namespace MovieMvcProject.Application.Interfaces.Caching
{
    public interface ICacheService
    {
        string GetFullKey(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }

}



