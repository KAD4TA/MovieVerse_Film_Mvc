using AutoMapper;
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Directors.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using System.Globalization;

namespace MovieMvcProject.Application.Features.Directors.Handlers
{
   
    public class GetDirectorForEditQueryHandler : IRequestHandler<GetDirectorForEditQuery, DirectorDetailDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        public GetDirectorForEditQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }
       
        public async Task<DirectorDetailDto?> Handle(GetDirectorForEditQuery request, CancellationToken ct)
        {
            var culture = CultureInfo.CurrentUICulture.Name ?? "tr-TR";
            var cacheKey = $"directors:edit:{request.DirectorId}:{culture}";

            var cached = await _cache.GetAsync<DirectorDetailDto>(cacheKey, ct);
            if (cached != null) return cached;

            var director = await _unitOfWork.Directors.GetByIdWithMoviesAsync(request.DirectorId);
            if (director == null) return null;

            var dto = _mapper.Map<DirectorDetailDto>(director);
            var movieDtos = _mapper.Map<List<MovieDtoResponse>>(director.DirectedMovies);
            dto.Movies = new PagedResult<MovieDtoResponse>(movieDtos, movieDtos.Count, 1, movieDtos.Count);

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), ct);
            return dto;
        }
    }
}
