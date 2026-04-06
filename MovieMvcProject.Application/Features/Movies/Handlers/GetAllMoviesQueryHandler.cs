

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Movies.Handlers;

public class GetAllMoviesQueryHandler : IRequestHandler<GetAllMoviesQuery, PagedResult<MovieDtoResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IElasticSearchService _elasticSearchService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllMoviesQueryHandler> _logger;
    private readonly IStringLocalizer<ExceptionResource> _localizer;

    private const string MoviesIndexName = "movies";

    public GetAllMoviesQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IElasticSearchService elasticSearchService,
        IMapper mapper,
        ILogger<GetAllMoviesQueryHandler> logger,
        IStringLocalizer<ExceptionResource> localizer)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _elasticSearchService = elasticSearchService;
        _mapper = mapper;
        _logger = logger;
        _localizer = localizer;
    }

    
    public async Task<PagedResult<MovieDtoResponse>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
    {
        
        if (string.IsNullOrWhiteSpace(request.LanguageCode))
            throw new BadRequestException(_localizer["LanguageCodeRequired"]?.Value ?? "Language code is required.");

        var safeSearchTerm = request.SearchTerm?.ToLowerInvariant() ?? "none";
        string cacheKey = $"movies:all:{request.LanguageCode}:{safeSearchTerm}:{request.PageNumber}:{request.PageSize}";

        var cached = await _cacheService.GetAsync<PagedResult<MovieDtoResponse>>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        List<MovieDtoResponse> dtos;
        int totalCount;

       
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
           
            var searchResult = await _elasticSearchService.SearchAsync<MovieSearchDocument>(
                indexName: MoviesIndexName,
                searchTerm: request.SearchTerm,
                searchFields: new[] { "titleTr", "titleEn" },
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                ct: cancellationToken);

            totalCount = searchResult.TotalCount;

           
            dtos = _mapper.Map<List<MovieDtoResponse>>(searchResult.Items, opt =>
            {
                opt.Items["LanguageCode"] = request.LanguageCode;
            });
        }
        else
        {
           
            var dbResult = await _unitOfWork.Movies.GetAllMovies(
                request.LanguageCode,
                null,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            totalCount = dbResult.TotalCount;

            dtos = _mapper.Map<List<MovieDtoResponse>>(dbResult.Items, opt =>
            {
                opt.Items["LanguageCode"] = request.LanguageCode;
            });
        }

        if (totalCount == 0)
            return PagedResult<MovieDtoResponse>.Empty(request.PageNumber, request.PageSize);

        var result = new PagedResult<MovieDtoResponse>(dtos, totalCount, request.PageNumber, request.PageSize);

       
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);

        return result;
    }
}