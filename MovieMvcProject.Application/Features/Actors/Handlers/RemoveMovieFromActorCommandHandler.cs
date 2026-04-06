
using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;

namespace MovieMvcProject.Application.Features.Actors.Commands.RemoveMovieFromActor
{
    public class RemoveMovieFromActorCommandHandler : IRequestHandler<RemoveMovieFromActorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;
        private readonly ILogger<RemoveMovieFromActorCommandHandler> _logger;

        public RemoveMovieFromActorCommandHandler(
            IUnitOfWork unitOfWork,
            ICacheService cache,
            ILogger<RemoveMovieFromActorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> Handle(RemoveMovieFromActorCommand request, CancellationToken ct)
        {
            try
            {
                
                await _unitOfWork.MovieActors.DeleteAsync(request.MovieId, request.ActorId);

             
                await _unitOfWork.SaveChangesAsync(ct);

               
                await InvalidateActorCaches(request.ActorId, ct);

                _logger.LogInformation("Film aktörden başarıyla kaldırıldı. MovieId: {MovieId}, ActorId: {ActorId}",
                    request.MovieId, request.ActorId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film aktörden kaldırılırken hata oluştu. MovieId: {MovieId}, ActorId: {ActorId}",
                    request.MovieId, request.ActorId);
                throw;
            }
        }

        private async Task InvalidateActorCaches(Guid actorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("actors:list:*", ct);
            await _cache.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            await _cache.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct);
        }
    }
}