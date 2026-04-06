

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Comments.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, CommentDtoResponse>
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<ExceptionResource> _localizer;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public GetCommentByIdQueryHandler(
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

        public async Task<CommentDtoResponse> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
        {
            var commentId = request.CommentId;
            var cacheKey = $"comment:detail:{commentId}";

            // 1. Cache Kontrolü
            var cachedDto = await _cacheService.GetAsync<CommentDtoResponse>(cacheKey, cancellationToken);
            if (cachedDto != null)
            {
                return cachedDto;
            }

            // 2. DB'den Veriyi Çekme (UoW üzerinden Repository)
            var commentEntity = await _unitOfWork.Comments.GetByIdAsync(commentId);

            // 3. Hata Yönetimi: Bulunamadığında
            if (commentEntity == null)
            {
                var msg = _localizer["CommentNotFound"] ?? $"Yorum ID {commentId} bulunamadı.";
                throw new NotFoundException(msg);
            }

            // 4. Haritalama (Entity -> DTO)
            var commentDto = _mapper.Map<CommentDtoResponse>(commentEntity);

            // 5. Cache'e yazma
            await _cacheService.SetAsync(cacheKey, commentDto, CacheDuration, cancellationToken);

            return commentDto;
        }
    }
}
