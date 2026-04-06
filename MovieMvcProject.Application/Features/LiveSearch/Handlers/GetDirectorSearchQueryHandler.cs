

using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.DTOs.LiveSearch;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.LiveSearch.Queries;
using MovieMvcProject.Application.Interfaces.Indexing;

public class GetDirectorSearchQueryHandler : IRequestHandler<GetDirectorSearchQuery, List<LiveSearchResultDto>>
{
    private readonly IElasticSearchService _elasticSearchService;
    private readonly ILogger<GetDirectorSearchQueryHandler> _logger;

    public GetDirectorSearchQueryHandler(IElasticSearchService elasticSearchService, ILogger<GetDirectorSearchQueryHandler> logger)
    {
        _elasticSearchService = elasticSearchService;
        _logger = logger;
    }

    public async Task<List<LiveSearchResultDto>> Handle(GetDirectorSearchQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
                return new List<LiveSearchResultDto>();

            var searchResult = await _elasticSearchService.SearchAsync<DirectorSearchDocument>(
                indexName: "directors",
                searchTerm: request.Query,
                searchFields: new[] { "name", "directorId" },
                pageNumber: 1,
                pageSize: request.PageSize,
                ct: cancellationToken);

            return searchResult.Items.Select(director => new LiveSearchResultDto(
                Id: director.DirectorId.ToString(),
                Title: director.Name,
                Type: "Yönetmen",
                Url: $"/Admin/AdminDirector/Edit/{director.DirectorId}",
                PhotoUrl: director.PhotoUrl
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Director Live Search failed");
            return new List<LiveSearchResultDto>();
        }
    }
}