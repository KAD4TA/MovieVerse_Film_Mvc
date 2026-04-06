
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
    public class GetAllDirectorsQueryHandler : IRequestHandler<GetAllDirectorsQuery, PagedResult<DirectorLookupDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        public GetAllDirectorsQueryHandler(IUnitOfWork unitOfWork, ICacheService cache, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _mapper = mapper;
        }
       

        public async Task<PagedResult<DirectorLookupDto>> Handle(GetAllDirectorsQuery request, CancellationToken ct)
        {
            var culture = CultureInfo.CurrentUICulture.Name ?? "tr-TR";
            var cacheKey = $"directors:list:{request.PageNumber}:{request.PageSize}:{(request.SearchTerm?.GetHashCode() ?? 0)}:{culture}";

            var cached = await _cache.GetAsync<PagedResult<DirectorLookupDto>>(cacheKey, ct);
            if (cached != null) return cached;

            var directors = await _unitOfWork.Directors.GetAllAsync();
            var query = directors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(d => d.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));

            var totalCount = query.Count();
            var pagedDirectors = query
                .OrderBy(d => d.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<DirectorLookupDto>>(pagedDirectors);
            var result = new PagedResult<DirectorLookupDto>(dtos, totalCount, request.PageNumber, request.PageSize);

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);
            return result;
        }
    }
}