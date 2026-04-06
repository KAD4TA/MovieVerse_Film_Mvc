using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.DTOs.LiveSearch;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.LiveSearch.Queries;
using MovieMvcProject.Application.Interfaces.Indexing;

namespace MovieMvcProject.Application.Features.LiveSearch.Handlers
{

    public class GetActorSearchQueryHandler : IRequestHandler<GetActorSearchQuery, List<LiveSearchResultDto>>
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ILogger<GetActorSearchQueryHandler> _logger;

        public GetActorSearchQueryHandler(IElasticSearchService elasticSearchService, ILogger<GetActorSearchQueryHandler> logger)
        {
            _elasticSearchService = elasticSearchService;
            _logger = logger;
        }

        public async Task<List<LiveSearchResultDto>> Handle(GetActorSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
                    return new List<LiveSearchResultDto>();

                var searchResult = await _elasticSearchService.SearchAsync<ActorSearchDocument>(
                    indexName: "actors",
                    searchTerm: request.Query,
                    searchFields: new[] { "name", "actorId" },           
                    pageNumber: 1,
                    pageSize: request.PageSize,
                    ct: cancellationToken);

                return searchResult.Items.Select(actor => new LiveSearchResultDto(
                    Id: actor.ActorId.ToString(),
                    Title: actor.Name,
                    Type: "Oyuncu",
                    Url: $"/Admin/AdminActor/Edit/{actor.ActorId}",
                    PhotoUrl: actor.PhotoUrl
                )).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Actor Live Search failed");
                return new List<LiveSearchResultDto>();
            }
        }
    }
}
