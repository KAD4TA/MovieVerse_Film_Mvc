

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Movies.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, MovieDtoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ILogger<CreateMovieCommandHandler> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<ExceptionResource> _localizer;

        public CreateMovieCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IElasticSearchService elasticSearchService,
            ILogger<CreateMovieCommandHandler> logger,
            ILocalizationService localizationService,
            IStringLocalizer<ExceptionResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _elasticSearchService = elasticSearchService;
            _logger = logger;
            _localizationService = localizationService;
            _localizer = localizer;
        }

        public async Task<MovieDtoResponse> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
        {
            if (request.MovieDto == null)
                throw new ArgumentNullException(nameof(request.MovieDto));

            try
            {
                // 1. Yönetmen İşleme
                var director = await HandleDirectorAsync(request.MovieDto, cancellationToken);

                // 2. Film Oluşturma
                var movie = _mapper.Map<Movie>(request.MovieDto);
                movie.Director = director;
                movie.DirectorId = director.DirectorId;

                // 3. Aktörleri İşleme
                await HandleActorsAsync(movie, request.MovieDto, cancellationToken);

                // 4. Kaydet
                await _unitOfWork.Movies.AddAsync(movie);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. Detaylarıyla yeniden yükle
                var reloadedMovie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(movie.MovieId);
                if (reloadedMovie == null)
                    throw new InvalidOperationException("Film kaydedildi ancak yüklenemedi.");

                // 6. Cache temizleme
                await InvalidateCachesAfterCreate(reloadedMovie, cancellationToken);

                // 7. Elasticsearch Indexing
                await IndexMovieAsync(reloadedMovie, cancellationToken);
                if (request.MovieDto.NewDirector != null)
                    await IndexDirectorAsync(director, cancellationToken);

                // 8. Response
                var language = _localizationService.GetCurrentLanguageCode() ?? "tr";
                return _mapper.Map<MovieDtoResponse>(reloadedMovie, opt => opt.Items["LanguageCode"] = language);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film oluşturulurken hata oluştu. DTO: {@MovieDto}", request.MovieDto);
                throw;
            }
        }

        #region Helper Metodlar

        private async Task<Director> HandleDirectorAsync(CreateMovieDto dto, CancellationToken ct)
        {
            if (dto.ExistingDirectorId.HasValue && dto.ExistingDirectorId.Value != Guid.Empty)
            {
                var director = await _unitOfWork.Directors.GetByIdAsync(dto.ExistingDirectorId.Value);
                if (director == null)
                    throw new NotFoundException("Seçilen yönetmen bulunamadı.", _localizer);
                return director;
            }

            if (dto.NewDirector != null)
            {
                var director = _mapper.Map<Director>(dto.NewDirector);
                director.DirectorId = Guid.NewGuid();
                await _unitOfWork.Directors.AddAsync(director);
                return director;
            }

            throw new ArgumentException("Film için bir yönetmen belirtilmelidir.");
        }

        private async Task HandleActorsAsync(Movie movie, CreateMovieDto dto, CancellationToken ct)
        {
            // Mevcut ActorIds
            if (dto.ExistingActorIds?.Any() == true)
            {
                foreach (var actorId in dto.ExistingActorIds)
                {
                    var actor = await _unitOfWork.Actors.GetByIdAsync(actorId);
                    if (actor == null)
                        throw new NotFoundException($"Aktör bulunamadı: {actorId}", _localizer);

                    movie.MovieActors.Add(new MovieActor { MovieId = movie.MovieId, ActorId = actorId });
                }
            }

            // Yeni + Mevcut Aktörler (DTO listesi)
            if (dto.Actors?.Any() == true)
            {
                foreach (var actorDto in dto.Actors)
                {
                    Actor actorEntity;

                    if (actorDto.ActorId != Guid.Empty) // Mevcut aktör
                    {
                        actorEntity = await _unitOfWork.Actors.GetByIdAsync(actorDto.ActorId);
                        if (actorEntity == null)
                            throw new NotFoundException($"Aktör bulunamadı: {actorDto.ActorId}", _localizer);
                    }
                    else // Yeni aktör
                    {
                        actorEntity = _mapper.Map<Actor>(actorDto);
                        actorEntity.ActorId = Guid.NewGuid();
                        await _unitOfWork.Actors.AddAsync(actorEntity);

                        // 🔥 YENİ AKTÖR İÇİN INDEXING 
                        var searchDoc = _mapper.Map<ActorSearchDocument>(actorEntity);
                        await _elasticSearchService.IndexAsync(searchDoc, "actors", actorEntity.ActorId, ct);
                    }

                    movie.MovieActors.Add(new MovieActor
                    {
                        MovieId = movie.MovieId,
                        Actor = actorEntity
                    });
                }
            }
        }

        private async Task InvalidateCachesAfterCreate(Movie movie, CancellationToken ct)
        {
            await _cacheService.RemoveByPatternAsync("movies:all:*", ct);
            await _cacheService.RemoveByPatternAsync("movies:search:*", ct);
            await _cacheService.RemoveByPatternAsync($"movie:detail:{movie.MovieId}:*", ct);

            // Director cache
            if (movie.DirectorId != Guid.Empty)
            {
                await _cacheService.RemoveByPatternAsync("directors:list:*", ct);
                await _cacheService.RemoveByPatternAsync($"directors:detail:{movie.DirectorId}:*", ct);
            }

            // Actor cache
            await _cacheService.RemoveByPatternAsync("actors:list:*", ct);
            var actorIds = movie.MovieActors.Select(ma => ma.ActorId).Distinct().ToList();
            foreach (var actorId in actorIds)
            {
                await _cacheService.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct);
                await _cacheService.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            }
        }

        private async Task IndexMovieAsync(Movie movie, CancellationToken ct)
        {
            var tr = movie.Translations?.FirstOrDefault(t => t.LanguageCode == "tr");
            var en = movie.Translations?.FirstOrDefault(t => t.LanguageCode == "en");

            var movieSearchDoc = new MovieSearchDocument(
                Id: movie.MovieId,
                TitleTr: tr?.Title ?? "Başlıksız",
                TitleEn: en?.Title ?? "Untitled",
                DescriptionTr: tr?.Description ?? "",
                DescriptionEn: en?.Description ?? "",
                Rating: movie.Rating,
                Category: movie.Category.ToString(),
                PosterPath: movie.PosterUrl ?? "",
                ReleaseYear: movie.Year
            );

            await _elasticSearchService.IndexAsync(movieSearchDoc, "movies", movie.MovieId, ct);
        }

        private async Task IndexDirectorAsync(Director director, CancellationToken ct)
        {
            var searchDoc = _mapper.Map<DirectorSearchDocument>(director);
            await _elasticSearchService.IndexAsync(searchDoc, "directors", director.DirectorId, ct);
        }

        #endregion
    }
}
