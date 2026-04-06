





using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Comments.Commands;
using MovieMvcProject.Application.Features.Comments.Events;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, DeleteCommentDtoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovieRatingService _movieRatingService;
        private readonly IMediator _mediator;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ICommentIndexingService _commentIndexingService;
        private readonly ICacheService _cacheService;
        private readonly IStringLocalizer<ExceptionResource> _localizer;
        private readonly ILogger<DeleteCommentCommandHandler> _logger;

        private const string MoviesIndexName = "movies";

        public DeleteCommentCommandHandler(
            IUnitOfWork unitOfWork,
            IMovieRatingService movieRatingService,
            IMediator mediator,
            IElasticSearchService elasticSearchService,
            ICommentIndexingService commentIndexingService,
            ICacheService cacheService,
            IStringLocalizer<ExceptionResource> localizer,
            ILogger<DeleteCommentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _movieRatingService = movieRatingService;
            _mediator = mediator;
            _elasticSearchService = elasticSearchService;
            _commentIndexingService = commentIndexingService;
            _cacheService = cacheService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<DeleteCommentDtoResponse> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var commentId = request.CommentDto.CommentId;

            // 1. Yorumu bulma
            var commentToDelete = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (commentToDelete == null)
            {
                throw new NotFoundException("Comment", commentId, _localizer);
            }

            var movieId = commentToDelete.MovieId;

            // 2. Veritabanı İşlemleri (Alt yorumları ve ana yorumu silmek için)
            try
            {
                
                var replies = await _unitOfWork.Comments.FindAsync(c => c.ParentId == commentId);

                if (replies != null && replies.Any())
                {
                    foreach (var reply in replies)
                    {
                        
                        await _commentIndexingService.DeleteIndexAsync(reply.CommentId, cancellationToken);
                        await _unitOfWork.Comments.DeleteAsync(reply.CommentId);
                    }
                    _logger.LogInformation("{Count} adet alt yorum siliniyor. ParentId: {ParentId}", replies.Count(), commentId);
                }

                // Ana yorumu sil
                await _unitOfWork.Comments.DeleteAsync(commentId);

                // Hepsini tek bir transaction (SaveChanges) ile tamamlama
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum silinirken DB hatası. CommentId: {CommentId}", commentId);
                throw new InternalServerException(_localizer["DatabaseDeleteError"] ?? "Veritabanından silme hatası.", ex);
            }

           
            try
            {
                // A. Elasticsearch'ten ana yorumu silme
                await _commentIndexingService.DeleteIndexAsync(commentId, cancellationToken);

                // B. Ortalama puanı yeniden hesaplama
                await _movieRatingService.CalculateAndUpdateAverageRatingAsync(movieId);

                // C. Film dokümanını ES'te güncelleme
                var updatedMovie = await _unitOfWork.Movies.GetByIdAsync(movieId);
                if (updatedMovie != null)
                {
                    var tr = updatedMovie.Translations?.FirstOrDefault(t => t.LanguageCode == "tr");
                    var en = updatedMovie.Translations?.FirstOrDefault(t => t.LanguageCode == "en");

                    var searchDoc = new MovieSearchDocument(
                        Id: updatedMovie.MovieId,
                        TitleTr: tr?.Title ?? "Başlıksız",
                        TitleEn: en?.Title ?? "Untitled",
                        DescriptionTr: tr?.Description ?? "",
                        DescriptionEn: en?.Description ?? "",
                        Rating: updatedMovie.Rating,
                        Category: updatedMovie.Category.ToString(),
                        PosterPath: updatedMovie.PosterUrl ?? "",
                        ReleaseYear: updatedMovie.Year
                    );

                    await _elasticSearchService.IndexAsync(searchDoc, MoviesIndexName, updatedMovie.MovieId, cancellationToken);
                }

                // D. Cache Temizleme
                await ClearRelatedCachesAsync(movieId, cancellationToken);

                // E. Event yayınlama
                await _mediator.Publish(new CommentChangedEvent(movieId), cancellationToken);

                _logger.LogInformation("Silme işlemi ve yan etkiler tamamlandı. MovieId: {MovieId}", movieId);
            }
            catch (Exception sideEx)
            {
                _logger.LogWarning(sideEx, "DB silindi ancak ES/Cache güncellenemedi. CommentId: {Id}", commentId);
            }

            return new DeleteCommentDtoResponse
            {
                Success = true,
                Message = _localizer["Comment_Deleted"] ?? "Yorum başarıyla silindi."
            };
        }

        private async Task ClearRelatedCachesAsync(Guid movieId, CancellationToken ct)
        {
            await _cacheService.RemoveByPatternAsync("comments:*", ct);

            // Film detayındaki yorumları da temizleme
            await _cacheService.RemoveByPatternAsync($"movie:detail:{movieId}:*", ct);

            // Genel film listelerini temizleme (puan değiştiği için)
            await _cacheService.RemoveByPatternAsync("movies:all:*", ct);
        }
    }
}