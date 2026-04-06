using MovieMvcProject.Application.Commons;

namespace MovieMvcProject.Application.Interfaces.Indexing
{
    
    public interface ICommentIndexingService
    {
        
        Task IndexAsync(Guid commentId, object commentData, CancellationToken ct);
        Task DeleteIndexAsync(Guid commentId, CancellationToken ct);
        Task<PagedResult<Guid>> SearchCommentIdsAsync(string query, int pageNumber, int pageSize, CancellationToken ct);
    }
}
