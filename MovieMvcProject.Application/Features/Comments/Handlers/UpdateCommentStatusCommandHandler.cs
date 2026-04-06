
using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Comments.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class UpdateCommentStatusCommandHandler : IRequestHandler<UpdateCommentStatusCommand, BaseResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommentIndexingService _commentIndexingService;
        private readonly ICacheService _cacheService;
        private readonly IMovieRatingService _movieRatingService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ILogger<UpdateCommentStatusCommandHandler> _logger;

        public UpdateCommentStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ICommentIndexingService commentIndexingService,
            ICacheService cacheService,
            IMovieRatingService movieRatingService,
            IElasticSearchService elasticSearchService,
            ILogger<UpdateCommentStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _commentIndexingService = commentIndexingService;
            _cacheService = cacheService;
            _movieRatingService = movieRatingService;
            _elasticSearchService = elasticSearchService;
            _logger = logger;
        }

        public async Task<BaseResponse> Handle(UpdateCommentStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Yorumu bulma ve güncelleme
                var comment = await _unitOfWork.Comments.GetByIdAsync(request.CommentId);
                if (comment == null)
                    return new BaseResponse { IsSuccess = false, Message = "Yorum bulunamadı." };

                var previousStatus = comment.Status;
                comment.Status = request.NewStatus;
                comment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cacheService.RemoveByPatternAsync("comments:*", cancellationToken);
                await _cacheService.RemoveByPatternAsync($"movie:detail:{comment.MovieId}:*", cancellationToken);

                // ====================== RATING + ES  ======================
                if (request.NewStatus == CommentStatus.Approved || previousStatus == CommentStatus.Approved)
                {
                    await _movieRatingService.CalculateAndUpdateAverageRatingAsync(comment.MovieId);

                    // Film dokümanını Elasticsearch'te güncelleme
                    try
                    {
                        var movie = await _unitOfWork.Movies.GetByIdAsync(comment.MovieId);
                        if (movie != null)
                        {
                            var tr = movie.Translations?.FirstOrDefault(t => t.LanguageCode == "tr");
                            var en = movie.Translations?.FirstOrDefault(t => t.LanguageCode == "en");

                            var movieSearchDoc = new MovieSearchDocument(
                                Id: movie.MovieId,
                                TitleTr: tr?.Title ?? "Başlıksız",
                                TitleEn: en?.Title ?? "Untitled",
                                DescriptionTr: tr?.Description ?? "",
                                DescriptionEn: en?.Description ?? "",
                                Rating: movie.Rating,
                                Category: movie.Category.ToString(),
                                PosterPath: movie.PosterUrl ?? "",
                                ReleaseYear: movie.Year);

                            await _elasticSearchService.IndexAsync(movieSearchDoc, "movies", movie.MovieId, cancellationToken);
                        }
                    }
                    catch (Exception esEx)
                    {
                        _logger.LogWarning(esEx, "Movie Elasticsearch index güncellenirken hata. MovieId: {MovieId}", comment.MovieId);
                    }
                }

                // ====================== YORUM ES + CACHE  ======================
                try
                {
                    var elasticData = new
                    {
                        id = comment.CommentId,
                        status = comment.Status.ToString(),
                        updatedAt = comment.UpdatedAt
                    };

                    await _commentIndexingService.IndexAsync(comment.CommentId, elasticData, cancellationToken);
                    await Task.Delay(500, cancellationToken);

                    _logger.LogInformation(
                        "Yorum durumu güncellendi (eski: {OldStatus} → yeni: {NewStatus}). Cache'ler temizlendi. CommentId: {CommentId}, MovieId: {MovieId}",
                        previousStatus, request.NewStatus, request.CommentId, comment.MovieId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Yorum DB güncellendi ama ES/Cache hatası. CommentId: {CommentId}", request.CommentId);
                }

                return new BaseResponse { IsSuccess = true, Message = "Yorum durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Yorum durumu güncellenirken beklenmedik hata. CommentId: {CommentId}", request.CommentId);
                return new BaseResponse { IsSuccess = false, Message = "Teknik bir hata oluştu." };
            }
        }
    }
}

