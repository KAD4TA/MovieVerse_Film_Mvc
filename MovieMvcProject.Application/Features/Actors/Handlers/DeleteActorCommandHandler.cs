
using MediatR;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;

namespace MovieMvcProject.Application.Features.Actors.Commands.DeleteActor
{
    public class DeleteActorCommandHandler : IRequestHandler<DeleteActorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elastic;
        private readonly ICacheService _cache;

        public DeleteActorCommandHandler(
            IUnitOfWork unitOfWork,
            IElasticSearchService elastic,
            ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _elastic = elastic;
            _cache = cache;
        }

        
        public async Task<bool> Handle(DeleteActorCommand request, CancellationToken ct)
        {
            var actor = await _unitOfWork.Actors.GetByIdAsync(request.ActorId);
            if (actor == null) return false;

            await _unitOfWork.Actors.DeleteAsync(request.ActorId);
            await _unitOfWork.SaveChangesAsync(ct);

            await _elastic.DeleteAsync("actors", request.ActorId.ToString(), ct);

            await InvalidateActorCaches(request.ActorId, ct);
            return true;
        }

        private async Task InvalidateActorCaches(Guid actorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("actors:list:*", ct);
            await _cache.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            await _cache.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct);
        }
    }
}
