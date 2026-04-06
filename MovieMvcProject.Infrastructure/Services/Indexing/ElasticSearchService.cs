

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Interfaces.Indexing;

namespace MovieMvcProject.Infrastructure.Services.Indexing;

public sealed class ElasticSearchService : IElasticSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchService> _logger;

    public ElasticSearchService(ElasticsearchClient client, ILogger<ElasticSearchService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task IndexAsync<T>(T entity, string indexName, object id, CancellationToken ct = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(indexName);
        ArgumentNullException.ThrowIfNull(id);

        var response = await _client.IndexAsync(entity, idx => idx
            .Index(indexName)
            .Id(id.ToString()!)
            .Refresh(Refresh.True), ct);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Indexleme hatası: {Debug}", response.DebugInformation);
            throw new InvalidOperationException($"Indexleme başarısız: {response.DebugInformation}");
        }
        _logger.LogInformation("Döküman yüklendi: {Index}/{Id}", indexName, id);
    }

    public async Task DeleteAsync(string indexName, object id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentNullException.ThrowIfNull(id);

        var deleteRequest = new DeleteRequest(indexName, id.ToString()!)
        {
            Refresh = Refresh.True
        };

        var response = await _client.DeleteAsync(deleteRequest, ct);

        if (!response.IsValidResponse &&
            (response.ApiCallDetails.HttpStatusCode == null || response.ApiCallDetails.HttpStatusCode != 404))
        {
            _logger.LogError("Elasticsearch silme hatası - Index: {Index}, Id: {Id}, Detay: {Debug}",
                indexName, id, response.DebugInformation);
            throw new InvalidOperationException($"Elasticsearch'ten doküman silinemedi: {response.DebugInformation}");
        }
        _logger.LogInformation("Elasticsearch'ten doküman silindi: {Index}/{Id}", indexName, id);
    }

    
    // Genel arama 
   
    public async Task<PagedResult<T>> SearchAsync<T>(
        string indexName,
        string searchTerm,
        string[] searchFields,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return PagedResult<T>.Empty(pageNumber, pageSize);
        if (searchFields == null || searchFields.Length == 0)
            throw new ArgumentException("En az bir arama alanı belirtilmelidir.", nameof(searchFields));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));
        var from = (pageNumber - 1) * pageSize;
        var trimmed = searchTerm.Trim();

        var response = await _client.SearchAsync<T>(s => s
            
            .From(from)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sh => sh.MultiMatch(mm => mm
                            .Fields(searchFields)
                            .Query(trimmed)
                            .Type(TextQueryType.BoolPrefix)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Boost(2.0f)
                        ),
                        sh => sh.QueryString(qs => qs
                            .Fields(searchFields)
                            .Query($"*{trimmed}*")
                            .DefaultOperator(Operator.And)
                        )
                    )
                    .MinimumShouldMatch(1)
                )
            )
            .TrackTotalHits(true), ct);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Elasticsearch arama hatası: {Debug}", response.DebugInformation);
            throw new InvalidOperationException($"Arama başarısız: {response.DebugInformation}");
        }

        var items = response.Documents.ToList();
        var total = response.Total > int.MaxValue ? int.MaxValue : (int)response.Total;
        return new PagedResult<T>(items, total, pageNumber, pageSize);
    }

    
    // Eski tek alanlı ID arama 
    
    public async Task<PagedResult<string>> SearchIdsByFieldAsync(
        string indexName,
        string searchTerm,
        string searchField,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return PagedResult<string>.Empty(pageNumber, pageSize);
        if (string.IsNullOrWhiteSpace(searchField))
            throw new ArgumentException("Arama alanı belirtilmelidir.", nameof(searchField));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));
        var from = (pageNumber - 1) * pageSize;
        var trimmed = searchTerm.Trim();

        var response = await _client.SearchAsync<CommentSearchDocument>(s => s

            .From(from)
            .Size(pageSize)
            .Query(q => q.Match(m => m
                .Field(searchField)
                .Query(trimmed)
            ))
            .Source(false)
            .TrackTotalHits(true), ct);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Elasticsearch ID arama hatası: {Debug}", response.DebugInformation);
            throw new InvalidOperationException($"ID arama başarısız: {response.DebugInformation}");
        }

        var ids = response.Hits.Select(h => h.Id).ToList();
        var total = response.Total > int.MaxValue ? int.MaxValue : (int)response.Total;
        return new PagedResult<string>(ids, total, pageNumber, pageSize);
    }

    
    //  Yazar adı + İçerik + Film adı üçünde birden arama 
    
    public async Task<PagedResult<string>> SearchIdsByMultiMatchAsync(
     string indexName,
     string query,
     Dictionary<string, float> fieldsWithBoost,
     int pageNumber,
     int pageSize,
     CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return PagedResult<string>.Empty(pageNumber, pageSize);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));
        var from = (pageNumber - 1) * pageSize;
        var trimmed = query.Trim();

       
        var response = await _client.SearchAsync<CommentSearchDocument>(s => s
            .From(from)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(new[]
                    {
                    $"content^{fieldsWithBoost.GetValueOrDefault("content", 1.0f)}",
                    $"username^{fieldsWithBoost.GetValueOrDefault("username", 3.0f)}",
                    $"movieTitle^{fieldsWithBoost.GetValueOrDefault("movieTitle", 2.0f)}"
                    })
                    .Query(trimmed)
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            )
            .TrackTotalHits(true), ct);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Elasticsearch multi-match arama hatası: {Debug}", response.DebugInformation);
            throw new InvalidOperationException($"Arama başarısız: {response.DebugInformation}");
        }

        var ids = response.Hits.Select(h => h.Id).ToList();
        var total = response.Total > int.MaxValue ? int.MaxValue : (int)response.Total;
        return new PagedResult<string>(ids, total, pageNumber, pageSize);
    }

    public async Task EnsureIndexAsync<T>(string indexName, CancellationToken ct = default) where T : class
    {
        var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);
        if (existsResponse.Exists)
            return;

        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("turkish_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding", "turkish_stop" })
                            .CharFilter(new[] { "html_strip" })
                        )
                    )
                    .TokenFilters(tf => tf
                        .Stop("turkish_stop", st => st
                            .Stopwords(new[] { "_turkish_" })
                            .IgnoreCase(true)
                        )
                    )
                )
            )
            .Mappings(m => m
                .DynamicTemplates(dt => dt
                    .Add("strings_as_turkish", d => d
                        .MatchMappingType("string")
                        .Mapping(mm => mm
                            .Text(tm => tm.Analyzer("turkish_analyzer"))
                        )
                    )
                )
            ), ct);

        if (!createResponse.IsValidResponse)
        {
            _logger.LogError("Index oluşturulamadı: {Error}", createResponse.DebugInformation);
            throw new InvalidOperationException($"Index oluşturulamadı: {createResponse.DebugInformation}");
        }
        _logger.LogInformation("Elasticsearch index oluşturuldu: {IndexName}", indexName);
    }
}