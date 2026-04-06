

using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Interfaces.Indexing;


public class CommentIndexingService : ICommentIndexingService
{
    private readonly IElasticSearchService _elasticSearchService;
    private const string CommentsIndexName = "comments";

    public CommentIndexingService(IElasticSearchService elasticSearchService)
    {
        _elasticSearchService = elasticSearchService;
    }

    public Task IndexAsync(Guid commentId, object commentData, CancellationToken ct)
    {
        return _elasticSearchService.IndexAsync(commentData, CommentsIndexName, commentId, ct);
    }

    public Task DeleteIndexAsync(Guid commentId, CancellationToken ct)
    {
        return _elasticSearchService.DeleteAsync(CommentsIndexName, commentId, ct);
    }

    
    public async Task<PagedResult<Guid>> SearchCommentIdsAsync(string query, int pageNumber, int pageSize, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return PagedResult<Guid>.Empty(pageNumber, pageSize);

        var fieldsWithBoost = new Dictionary<string, float>
    {
        { "content", 1.0f },
        { "username", 3.0f },    
        { "movieTitle", 2.0f }
    };

        var stringResult = await _elasticSearchService.SearchIdsByMultiMatchAsync(
            CommentsIndexName, query.Trim(), fieldsWithBoost, pageNumber, pageSize, ct);

        var guidItems = stringResult.Items
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        return new PagedResult<Guid>(guidItems, stringResult.TotalCount, pageNumber, pageSize);
    }
}
