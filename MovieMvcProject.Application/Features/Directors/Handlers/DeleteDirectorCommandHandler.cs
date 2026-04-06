using MediatR;
using MovieMvcProject.Application.Features.Directors.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;

namespace MovieMvcProject.Application.Features.Directors.Handlers
{
    public class DeleteDirectorCommandHandler : IRequestHandler<DeleteDirectorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elastic;
        private readonly ICacheService _cache;

        public DeleteDirectorCommandHandler(IUnitOfWork unitOfWork, IElasticSearchService elastic, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _elastic = elastic;
            _cache = cache;
        }

        public async Task<bool> Handle(DeleteDirectorCommand request, CancellationToken ct)
        {
            var director = await _unitOfWork.Directors.GetByIdAsync(request.DirectorId);
            if (director == null) return false;

            await _unitOfWork.Directors.DeleteAsync(request.DirectorId);
            await _unitOfWork.SaveChangesAsync(ct);

            await _elastic.DeleteAsync("directors", request.DirectorId.ToString(), ct);

            await InvalidateDirectorCaches(request.DirectorId, ct);
            return true;
        }

        private async Task InvalidateDirectorCaches(Guid directorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("directors:list:*", ct);
            await _cache.RemoveByPatternAsync($"directors:edit:{directorId}:*", ct);
            await _cache.RemoveByPatternAsync($"directors:detail:{directorId}:*", ct);
        }
    }
}
