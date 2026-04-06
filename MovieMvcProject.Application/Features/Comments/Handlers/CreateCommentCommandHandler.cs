




using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
using MovieMvcProject.Application.Interfaces.Notification;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Identity;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDtoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovieRatingService _movieRatingService;
        private readonly IMediator _mediator;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ICommentIndexingService _commentIndexingService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<ExceptionResource> _localizer;
        private readonly ILogger<CreateCommentCommandHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public CreateCommentCommandHandler(
            IUnitOfWork unitOfWork,
            IMovieRatingService movieRatingService,
            IMediator mediator,
            IElasticSearchService elasticSearchService,
            ICommentIndexingService commentIndexingService,
            ICacheService cacheService,
            IMapper mapper,
            IStringLocalizer<ExceptionResource> localizer,
            ILogger<CreateCommentCommandHandler> logger,
            INotificationService notificationService,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _movieRatingService = movieRatingService;
            _mediator = mediator;
            _elasticSearchService = elasticSearchService;
            _commentIndexingService = commentIndexingService;
            _cacheService = cacheService;
            _mapper = mapper;
            _localizer = localizer;
            _logger = logger;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<CommentDtoResponse> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var movieId = request.CommentDto.MovieId;
            var userId = request.UserId;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

            var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
            if (movie == null)
                throw new NotFoundException(nameof(movie), movieId, _localizer);

            // 1. Veritabanına Kayıt
            var commentEntity = _mapper.Map<Comment>(request.CommentDto);
            commentEntity.UserId = userId;
            commentEntity.CreatedAt = DateTime.Now;
            commentEntity.Status = CommentStatus.Pending;

            await _unitOfWork.Comments.AddAsync(commentEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

               
                await _movieRatingService.CalculateAndUpdateAverageRatingAsync(movieId);

                
                var updatedMovie = await _unitOfWork.Movies.GetByIdAsync(movieId);
                if (updatedMovie != null)
                {
                    var tr = updatedMovie.Translations?.FirstOrDefault(t => t.LanguageCode == "tr");
                    var en = updatedMovie.Translations?.FirstOrDefault(t => t.LanguageCode == "en");

                    var movieSearchDoc = new MovieSearchDocument(
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
                    await _elasticSearchService.IndexAsync(movieSearchDoc, "movies", updatedMovie.MovieId, cancellationToken);
                }

                // C. Elasticsearch Yorum Indexleme
                var elasticCommentData = new
                {
                    CommentId = commentEntity.CommentId,
                    Content = commentEntity.Content,
                    MovieId = commentEntity.MovieId,
                    UserId = commentEntity.UserId,
                    Username = user?.UserName ?? "Anonim",
                    UserProfileImageUrl = user?.ProfileImageUrl ?? "/profile-images/default-profile.png",
                    Status = commentEntity.Status.ToString(),
                    Parent = commentEntity.ParentId,

                    CreatedAt = commentEntity.CreatedAt
                };
                await _commentIndexingService.IndexAsync(commentEntity.CommentId, elasticCommentData, cancellationToken);

               


                await _cacheService.RemoveByPatternAsync($"movie:detail:{movieId}:*", cancellationToken);

                
                await _cacheService.RemoveByPatternAsync("comments:list:*", cancellationToken);

               
                await _cacheService.RemoveByPatternAsync("movies:all:*", cancellationToken);
                // -------------------------------------------------------------

                // E. Bildirim ve Event
                await _mediator.Publish(new CommentChangedEvent(movieId), cancellationToken);
                await _notificationService.NotifyAdminAsync($"{user?.UserName} yeni yorum yaptı.", "Yeni Yorum");

                _logger.LogInformation("Cache'ler (TR/EN) temizlendi. Film ortalaması güncellendi: {MovieId}", movieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yan işlemler sırasında hata oluştu. MovieId: {MovieId}", movieId);
            }

            return _mapper.Map<CommentDtoResponse>(commentEntity);
        }
    }
}






