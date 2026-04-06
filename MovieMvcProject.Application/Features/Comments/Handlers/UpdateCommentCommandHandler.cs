

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Comments.Commands;
using MovieMvcProject.Application.Features.Comments.Events;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDtoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovieRatingService _movieRatingService;
        private readonly IMediator _mediator;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ICommentIndexingService _commentIndexingService; 
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<ExceptionResource> _localizer;
        private const string MoviesIndexName = "movies";

        public UpdateCommentCommandHandler(
            IUnitOfWork unitOfWork, 
            IMovieRatingService movieRatingService,
            IMediator mediator,
            IElasticSearchService elasticSearchService,
            ICommentIndexingService commentIndexingService,
            ICacheService cacheService,
            IMapper mapper,
            IStringLocalizer<ExceptionResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _movieRatingService = movieRatingService;
            _mediator = mediator;
            _elasticSearchService = elasticSearchService;
            _commentIndexingService = commentIndexingService;
            _cacheService = cacheService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<CommentDtoResponse> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var commentDto = request.CommentDto;
            var commentId = commentDto.CommentId;
            var movieId = commentDto.MovieId;

            // 1. Yorumu ve İlişkili Verileri Getirme
            var existingComment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (existingComment == null) throw new NotFoundException(nameof(existingComment), commentId, _localizer);

            // 2. Yorum Güncelleme ve Onayı Geri Çekme
            existingComment.Content = commentDto.Content;
            existingComment.MovieReview = commentDto.MovieReview ?? 0;
            existingComment.UpdatedAt = DateTime.Now;
            existingComment.Status = CommentStatus.Pending;

            await _unitOfWork.Comments.UpdateAsync(existingComment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Film Verisini Getirme (Translations ile birlikte)
            
            var updatedMovie = await _unitOfWork.Movies.GetByIdAsync(movieId);

            
            var titleTr = updatedMovie?.Translations.FirstOrDefault(t => t.LanguageCode == "tr")?.Title ?? "Başlık Yok";
            var titleEn = updatedMovie?.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.Title ?? "No Title";

            // 4. ELASTICSEARCH - YORUM GÜNCELLEME
            var elasticCommentDoc = new CommentSearchDocument
            {
                CommentId = existingComment.CommentId,
                Content = existingComment.Content,
                Username = existingComment.User?.UserName ?? "Anonim",
                UserProfileImageUrl = existingComment.User?.ProfileImageUrl ?? "/images/default-avatar.png",
                MovieId = existingComment.MovieId,
                MovieTitle = titleTr, 
                UserId = existingComment.UserId,
                CreatedAt = existingComment.CreatedAt,
                Status = existingComment.Status.ToString(),
                MovieReview = (int)existingComment.MovieReview
            };
            await _commentIndexingService.IndexAsync(existingComment.CommentId, elasticCommentDoc, cancellationToken);

            // 5. FILM PUANINI GÜNCELLEME
            await _movieRatingService.CalculateAndUpdateAverageRatingAsync(movieId);

            // 6. ELASTICSEARCH - FILM GÜNCELLEME 
            if (updatedMovie != null)
            {
                
                var elasticMovieData = new
                {
                    MovieId = updatedMovie.MovieId,
                    Year = updatedMovie.Year,
                    Rating = updatedMovie.Rating,
                    Category = updatedMovie.Category.ToString(),
                    PosterUrl = updatedMovie.PosterUrl,
                    TitleTr = titleTr,
                    TitleEn = titleEn,
                    DescriptionTr = updatedMovie.Translations.FirstOrDefault(t => t.LanguageCode == "tr")?.Description ?? "",
                    DescriptionEn = updatedMovie.Translations.FirstOrDefault(t => t.LanguageCode == "en")?.Description ?? ""
                };

                await _elasticSearchService.IndexAsync(elasticMovieData, MoviesIndexName, updatedMovie.MovieId, cancellationToken);

                // 7. CACHE TEMİZLEME
                await _cacheService.RemoveByPatternAsync($"movie:detail:{movieId}:*", cancellationToken);
                await _cacheService.RemoveByPatternAsync($"comments:movie:{movieId}:*", cancellationToken);
            }

            await _mediator.Publish(new CommentChangedEvent(movieId), cancellationToken);
            return _mapper.Map<CommentDtoResponse>(existingComment);
        }
    }
}