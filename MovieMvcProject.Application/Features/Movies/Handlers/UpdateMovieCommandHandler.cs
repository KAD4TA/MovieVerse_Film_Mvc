
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
    public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, MovieDtoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ILogger<UpdateMovieCommandHandler> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<ExceptionResource> _localizer;

        public UpdateMovieCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IElasticSearchService elasticSearchService,
            ILogger<UpdateMovieCommandHandler> logger,
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

        public async Task<MovieDtoResponse> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
        {
            if (request.MovieDto == null)
                throw new ArgumentNullException(nameof(request.MovieDto));

            try
            {
                // 1️ Mevcut filmi yükleme
                var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieDto.MovieId);
                if (movie == null)
                    throw new NotFoundException("Film bulunamadı.", _localizer);

                // 2️ Yönetmen belirleme
                var director = await HandleDirectorAsync(request.MovieDto, cancellationToken);

                // 3️ Film ana alanlarını güncelleme
                _mapper.Map(request.MovieDto, movie);
                movie.Director = director;
                movie.DirectorId = director.DirectorId;

                // 4️ Aktörleri sıfırlama ve yeniden kurma
                await HandleActorsAsync(movie, request.MovieDto);

                // 5️ Kaydet
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 6️ Tüm ilişkilerle yeniden yükleme
                var reloadedMovie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(movie.MovieId);
                if (reloadedMovie == null)
                    throw new InvalidOperationException("Film güncellendi ancak yüklenemedi.");

                // 7️ Cache temizleme (movie + director + actor)
                await InvalidateMovieAndRelatedCaches(reloadedMovie, cancellationToken);

                // 8️ Elasticsearch → MANUAL 
                var tr = reloadedMovie.Translations?.FirstOrDefault(t => t.LanguageCode == "tr");
                var en = reloadedMovie.Translations?.FirstOrDefault(t => t.LanguageCode == "en");

                var movieSearchDoc = new MovieSearchDocument(
                    Id: reloadedMovie.MovieId,
                    TitleTr: tr?.Title ?? "Başlıksız",
                    TitleEn: en?.Title ?? "Untitled",
                    DescriptionTr: tr?.Description ?? "",
                    DescriptionEn: en?.Description ?? "",
                    Rating: reloadedMovie.Rating,
                    Category: reloadedMovie.Category.ToString(),
                    PosterPath: reloadedMovie.PosterUrl ?? "",
                    ReleaseYear: reloadedMovie.Year
                );

                await _elasticSearchService.IndexAsync(movieSearchDoc, "movies", reloadedMovie.MovieId, cancellationToken);

                // 9️ Response
                var language = _localizationService.GetCurrentLanguageCode() ?? "tr";
                return _mapper.Map<MovieDtoResponse>(reloadedMovie, opt => opt.Items["LanguageCode"] = language);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film güncellenirken hata oluştu. MovieId: {MovieId}", request.MovieDto.MovieId);
                throw;
            }
        }

        // ====================== HELPER METODLAR ======================

        private async Task<Director> HandleDirectorAsync(UpdateMovieDto dto, CancellationToken ct)
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

        private async Task HandleActorsAsync(Movie movie, UpdateMovieDto dto)
        {
            movie.MovieActors.Clear();

            // 1. Mevcut ActorIds
            if (dto.ExistingActorIds?.Any() == true)
            {
                foreach (var actorId in dto.ExistingActorIds)
                {
                    var actor = await _unitOfWork.Actors.GetByIdAsync(actorId);
                    if (actor == null)
                        throw new NotFoundException($"Aktör bulunamadı: {actorId}", _localizer);

                    movie.MovieActors.Add(new MovieActor
                    {
                        MovieId = movie.MovieId,
                        ActorId = actorId
                    });
                }
            }

            // 2. Actors listesi (hem mevcut hem yeni)
            if (dto.Actors?.Any() == true)
            {
                foreach (var actorDto in dto.Actors)
                {
                    Actor actorEntity;

                    if (actorDto.ActorId != Guid.Empty) //  MEVCUT AKTÖR
                    {
                        actorEntity = await _unitOfWork.Actors.GetByIdAsync(actorDto.ActorId);
                        if (actorEntity == null)
                            throw new NotFoundException($"Aktör bulunamadı: {actorDto.ActorId}", _localizer);
                       
                    }
                    else //  YENİ AKTÖR
                    {
                        actorEntity = _mapper.Map<Actor>(actorDto);
                        actorEntity.ActorId = Guid.NewGuid();
                        await _unitOfWork.Actors.AddAsync(actorEntity);
                    }

                    movie.MovieActors.Add(new MovieActor
                    {
                        MovieId = movie.MovieId,
                        Actor = actorEntity
                    });
                }
            }
        }

        private async Task InvalidateMovieAndRelatedCaches(Movie movie, CancellationToken ct)
        {
            // Movie cache'leri
            await _cacheService.RemoveByPatternAsync("movies:all:*", ct);
            await _cacheService.RemoveByPatternAsync("movies:search:*", ct);
            await _cacheService.RemoveByPatternAsync($"movie:detail:{movie.MovieId}:*", ct);

            // Director cache'leri
            if (movie.DirectorId != Guid.Empty)
            {
                await _cacheService.RemoveByPatternAsync("directors:list:*", ct);
                await _cacheService.RemoveByPatternAsync($"directors:detail:{movie.DirectorId}:*", ct);
            }

            // ==================== ACTOR CACHE'LERİ ====================
            await _cacheService.RemoveByPatternAsync("actors:list:*", ct);
            var actorIds = movie.MovieActors.Select(ma => ma.ActorId).Distinct().ToList();
            foreach (var actorId in actorIds)
            {
                await _cacheService.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct);
                await _cacheService.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            }
        }
    }
}