using MovieMvcProject.Application.Commons;

namespace MovieMvcProject.Application.Interfaces.Indexing;

public interface IElasticSearchService
{
    Task EnsureIndexAsync<T>(string indexName, CancellationToken ct = default) where T : class;

    Task IndexAsync<T>(T entity, string indexName, object id, CancellationToken ct = default) where T : class;

    
    Task DeleteAsync(string indexName, object id, CancellationToken ct = default);

    
    Task<PagedResult<T>> SearchAsync<T>(
        string indexName,
        string searchTerm,
        string[] searchFields,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default) where T : class;




    Task<PagedResult<string>> SearchIdsByFieldAsync(
        string indexName,
        string searchTerm,
        string searchField, 
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<PagedResult<string>> SearchIdsByMultiMatchAsync(
    string indexName,
    string query,
    Dictionary<string, float> fieldsWithBoost,  
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);
}