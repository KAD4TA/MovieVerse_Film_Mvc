
using AutoMapper;
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using System.Globalization;

public class SearchCommentsQueryHandler : IRequestHandler<SearchCommentsQuery, PagedResult<CommentDtoResponse>>
{
    private readonly ICommentIndexingService _indexingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    private const string CacheKeyPrefix = "comments:search";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public SearchCommentsQueryHandler(
        ICommentIndexingService indexingService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IMapper mapper)
    {
        _indexingService = indexingService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<PagedResult<CommentDtoResponse>> Handle(SearchCommentsQuery request, CancellationToken cancellationToken)
    {
      
        string currentLang = CultureInfo.CurrentCulture.Name.StartsWith("tr", StringComparison.OrdinalIgnoreCase)
            ? "tr" : "en";

        var cacheKey = $"{CacheKeyPrefix}:{request.Query}:{request.PageNumber}:{request.PageSize}:{currentLang}";

        var cached = await _cacheService.GetAsync<PagedResult<CommentDtoResponse>>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var searchResult = await _indexingService.SearchCommentIdsAsync(
            request.Query, request.PageNumber, request.PageSize, cancellationToken);

        if (!searchResult.Items.Any())
        {
            var empty = PagedResult<CommentDtoResponse>.Empty(request.PageNumber, request.PageSize);
            await _cacheService.SetAsync(cacheKey, empty, CacheDuration, cancellationToken);
            return empty;
        }

        var commentIds = searchResult.Items.ToList();
        var comments = await _unitOfWork.Comments.GetCommentsByIdsAsync(commentIds);

        var commentDtos = _mapper.Map<List<CommentDtoResponse>>(comments, opt =>
        {
            opt.Items["LanguageCode"] = currentLang;  
        });

        var result = new PagedResult<CommentDtoResponse>(
            commentDtos,
            searchResult.TotalCount,
            request.PageNumber,
            request.PageSize);

        await _cacheService.SetAsync(cacheKey, result, CacheDuration, cancellationToken);
        return result;
    }
}