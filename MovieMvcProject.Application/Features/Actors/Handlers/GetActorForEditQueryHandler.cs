using AutoMapper;
using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Actors.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;

namespace MovieMvcProject.Application.Features.Actors.Handlers
{
   

    public class GetActorForEditQueryHandler : IRequestHandler<GetActorForEditQuery, ActorEditDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public GetActorForEditQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        
        public async Task<ActorEditDto?> Handle(GetActorForEditQuery request, CancellationToken ct)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture.Name?.ToLowerInvariant();
            var cacheKey = $"actors:edit:{request.ActorId}:{culture}";

            var cached = await _cache.GetAsync<ActorEditDto>(cacheKey, ct);
            if (cached != null) return cached;

            var actor = await _unitOfWork.Actors.GetByIdAsync(request.ActorId);
            if (actor == null) return null;

            var dto = _mapper.Map<ActorEditDto>(actor);
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
