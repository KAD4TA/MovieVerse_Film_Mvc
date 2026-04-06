using AutoMapper;
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Actors.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;

namespace MovieMvcProject.Application.Features.Actors.Handlers
{
   
    public class GetAllActorsQueryHandler : IRequestHandler<GetAllActorsQuery, PagedResult<ActorListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;

        public GetAllActorsQueryHandler(IUnitOfWork unitOfWork, ICacheService cache, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _mapper = mapper;
        }



        public async Task<PagedResult<ActorListDto>> Handle(GetAllActorsQuery request, CancellationToken ct)
        {
            
            var culture = System.Globalization.CultureInfo.CurrentUICulture.Name?.ToLowerInvariant() ?? "tr-tr";
            var searchHash = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? 0
                : request.SearchTerm.ToLowerInvariant().GetHashCode();

            var cacheKey = $"actors:list:{culture}:{request.PageNumber}:{request.PageSize}:{searchHash}";

            var cached = await _cache.GetAsync<PagedResult<ActorListDto>>(cacheKey, ct);
            if (cached != null) return cached;

            var actors = await _unitOfWork.Actors.GetAllAsync();

            var query = actors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(a => a.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));

            var totalCount = query.Count();
            var pagedActors = query
                .OrderBy(a => a.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<ActorListDto>>(pagedActors);
            var result = new PagedResult<ActorListDto>(dtos, totalCount, request.PageNumber, request.PageSize);

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

            return result;
        }
    }
}
