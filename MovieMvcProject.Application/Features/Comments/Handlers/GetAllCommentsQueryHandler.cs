

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Comments.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Domain.Resources;
using System.Globalization;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class GetAllCommentsQueryHandler : IRequestHandler<GetAllCommentsQuery, PagedResult<CommentDtoResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<ExceptionResource> _localizer;

        private const string CacheKeyPrefix = "comments:list";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetAllCommentsQueryHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IMapper mapper,
            IStringLocalizer<ExceptionResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<PagedResult<CommentDtoResponse>> Handle(
            GetAllCommentsQuery request,
            CancellationToken cancellationToken)
        {
            string currentLang = CultureInfo.CurrentCulture.Name.StartsWith("tr", StringComparison.OrdinalIgnoreCase)
        ? "tr" : "en";

            var cacheKey = $"{CacheKeyPrefix}:{request.PageNumber}:{request.PageSize}:" +
                           $"{(string.IsNullOrWhiteSpace(request.SearchTerm) ? "none" : request.SearchTerm)}:" +
                           $"{(request.Status?.ToString() ?? "all")}:{currentLang}";   

            var cached = await _cacheService.GetAsync<PagedResult<CommentDtoResponse>>(cacheKey, cancellationToken);
            if (cached != null) return cached;

            // Parent yorumları getirme (sayfalama burada)
            var pagedParents = await _unitOfWork.Comments.GetAllComments(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.Status);

            if (pagedParents == null || !pagedParents.Items.Any())
            {
                return PagedResult<CommentDtoResponse>.Empty(request.PageNumber, request.PageSize);
            }

            // Parent yorumların ID'lerini alma
            var parentIds = pagedParents.Items.Select(x => x.CommentId).ToList();

            // Cevap yorumlarını getirme
            var replies = await _unitOfWork.Comments.FindAsync(
                c => c.ParentId != null && parentIds.Contains(c.ParentId.Value));

            // Tüm yorumları birleştirme
            var allComments = pagedParents.Items.Concat(replies).ToList();

            // Mapping sırasında dil kodunu context'e ekleme (Resolver bunu kullanacak)
            var flatDtos = _mapper.Map<List<CommentDtoResponse>>(allComments, opt =>
            {
                // Mevcut culture'dan dil kodunu belirleme
                string currentLang = CultureInfo.CurrentCulture.Name.StartsWith("tr", StringComparison.OrdinalIgnoreCase)
                    ? "tr"
                    : "en";

                

                opt.Items["LanguageCode"] = currentLang;
            });

            // Yorum ağacını oluşturmak için (nested replies)
            var tree = CommentTreeBuilder.BuildTree(flatDtos);

            // Sonuç nesnesi
            var result = new PagedResult<CommentDtoResponse>(
                tree,
                pagedParents.TotalCount,
                request.PageNumber,
                request.PageSize);

            // Cache'e kaydet
            await _cacheService.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

            return result;
        }
    }
}
