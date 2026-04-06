

using AutoMapper;
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.ILocalization;

namespace MovieMvcProject.Application.Features.Actors.Queries.GetActorWithMovies
{
    public class GetActorWithMoviesHandler : IRequestHandler<GetActorWithMoviesQuery, ActorDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheService _cache;

        public GetActorWithMoviesHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationService localizationService,
            ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
            _cache = cache;
        }

        public async Task<ActorDetailDto> Handle(GetActorWithMoviesQuery request, CancellationToken ct)
        {
            
            var languageCode = _localizationService.GetCurrentLanguageCode() ?? "tr";
            var cacheKey = $"actors:detail:{request.ActorId}:{languageCode}:{request.PageNumber}:{request.PageSize}";

            var cached = await _cache.GetAsync<ActorDetailDto>(cacheKey, ct);
            if (cached != null) return cached;

            var actor = await _unitOfWork.Actors.GetActorWithMoviesAsync(request.ActorId);
            if (actor == null)
                throw new InvalidOperationException("Actor not found");

            var dto = _mapper.Map<ActorDetailDto>(actor, opt =>
            {
                opt.Items["LanguageCode"] = languageCode;
            });

            
            var allMovies = actor.MovieActors.Select(ma => ma.Movie).Where(m => m != null).ToList();
            var totalCount = allMovies.Count;
            var pagedMovies = allMovies
                .OrderByDescending(m => m.Year)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var movieDtos = _mapper.Map<List<MovieDtoResponse>>(pagedMovies, opt =>
            {
                opt.Items["LanguageCode"] = languageCode;
            });

            dto.Movies = new PagedResult<MovieDtoResponse>(movieDtos, totalCount, request.PageNumber, request.PageSize);

            await InvalidateActorCaches(actor.ActorId, ct);
            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), ct);

            return dto;
        }

        private async Task InvalidateActorCaches(Guid actorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("actors:list:*", ct);
            await _cache.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            await _cache.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct); 
        }
    }
}