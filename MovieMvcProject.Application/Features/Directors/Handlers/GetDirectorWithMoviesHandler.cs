using AutoMapper;
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Directors.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.ILocalization;
using System.Globalization;

namespace MovieMvcProject.Application.Features.Directors.Handlers
{
    
    public class GetDirectorWithMoviesHandler : IRequestHandler<GetDirectorWithMoviesQuery, DirectorDetailDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheService _cache; 

        public GetDirectorWithMoviesHandler(IUnitOfWork uow, IMapper mapper, ILocalizationService localizationService, ICacheService cache)
        {
            _uow = uow;
            _mapper = mapper;
            _localizationService = localizationService;
            _cache = cache;
        }

        public async Task<DirectorDetailDto> Handle(GetDirectorWithMoviesQuery request, CancellationToken ct)
        {
            var culture = CultureInfo.CurrentUICulture.Name ?? "tr-TR";
            var cacheKey = $"directors:detail:{request.DirectorId}:{culture}:{request.PageNumber}:{request.PageSize}";

            var cached = await _cache.GetAsync<DirectorDetailDto>(cacheKey, ct);
            if (cached != null) return cached;

            var director = await _uow.Directors.GetByIdWithMoviesAsync(request.DirectorId);
            if (director == null) throw new Exception("Director not found");

            var dto = _mapper.Map<DirectorDetailDto>(director, opt =>
                opt.Items["LanguageCode"] = _localizationService.GetCurrentLanguageCode() ?? "tr");

            var allMovies = director.DirectedMovies ?? new List<Movie>();
            var totalCount = allMovies.Count;

            var pagedMovies = allMovies
                .OrderByDescending(m => m.Year)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var movieDtos = _mapper.Map<List<MovieDtoResponse>>(pagedMovies, opt =>
                opt.Items["LanguageCode"] = _localizationService.GetCurrentLanguageCode() ?? "tr");

            dto.Movies = new PagedResult<MovieDtoResponse>(movieDtos, totalCount, request.PageNumber, request.PageSize);

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), ct);
            return dto;
        }
    }
}
