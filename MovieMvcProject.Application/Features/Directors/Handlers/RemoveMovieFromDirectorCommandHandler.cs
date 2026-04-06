
using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;

namespace MovieMvcProject.Application.Features.Directors.Commands
{
    public class RemoveMovieFromDirectorCommandHandler
        : IRequestHandler<RemoveMovieFromDirectorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;
        private readonly ILogger<RemoveMovieFromDirectorCommandHandler> _logger;

        public RemoveMovieFromDirectorCommandHandler(
            IUnitOfWork unitOfWork,
            ICacheService cache,
            ILogger<RemoveMovieFromDirectorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> Handle(RemoveMovieFromDirectorCommand request, CancellationToken ct)
        {
            try
            {
                var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
                if (movie == null)
                {
                    _logger.LogWarning("Film bulunamadı. MovieId: {MovieId}", request.MovieId);
                    return false;
                }

                if (movie.DirectorId != request.DirectorId)
                {
                    _logger.LogWarning("Film bu yönetmene ait değil. MovieId: {MovieId}, DirectorId: {DirectorId}",
                        request.MovieId, request.DirectorId);
                    return false;
                }

                // İlişkiyi kaldırma
                movie.DirectorId = null;
                movie.Director = null;

                await _unitOfWork.SaveChangesAsync(ct);

                await InvalidateDirectorCaches(request.DirectorId, ct);

                _logger.LogInformation("Film yönetmenden kaldırıldı. MovieId: {MovieId}", request.MovieId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film yönetmenden kaldırılırken hata. MovieId: {MovieId}, DirectorId: {DirectorId}",
                    request.MovieId, request.DirectorId);
                throw;
            }
        }

        private async Task InvalidateDirectorCaches(Guid directorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("directors:list:*", ct);
            await _cache.RemoveByPatternAsync($"directors:edit:{directorId}:*", ct);
            await _cache.RemoveByPatternAsync($"directors:detail:{directorId}:*", ct);
        }
    }
}