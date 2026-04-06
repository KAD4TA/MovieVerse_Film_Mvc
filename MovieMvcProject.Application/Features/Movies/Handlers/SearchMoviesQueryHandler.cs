
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class SearchMoviesQueryHandler : IRequestHandler<SearchMoviesQuery, PagedResult<MovieDtoResponse>>
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchMoviesQueryHandler> _logger;
        private readonly IStringLocalizer<ExceptionResource> _localizer;
        

        public SearchMoviesQueryHandler(
            IElasticSearchService elasticSearchService,
            IMapper mapper,
            ILogger<SearchMoviesQueryHandler> logger,
            IStringLocalizer<ExceptionResource> localizer)
        {
            _elasticSearchService = elasticSearchService;
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
        }


        public async Task<PagedResult<MovieDtoResponse>> Handle(SearchMoviesQuery request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return PagedResult<MovieDtoResponse>.Empty(request.PageNumber, request.PageSize);

            var result = await _elasticSearchService.SearchAsync<MovieSearchDocument>(
                indexName: "movies",
                searchTerm: request.Query,
                searchFields: new[] { "titleTr", "titleEn", "descriptionTr", "descriptionEn", "id" },
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                ct);

            if (result.Items == null || !result.Items.Any())
                return PagedResult<MovieDtoResponse>.Empty(request.PageNumber, request.PageSize);

            var dtos = _mapper.Map<List<MovieDtoResponse>>(result.Items, opt =>
                opt.Items["LanguageCode"] = request.LanguageCode ?? "tr");

            return new PagedResult<MovieDtoResponse>(dtos, result.TotalCount, request.PageNumber, request.PageSize);
        }
    }
}