

using MediatR;
using MovieMvcProject.Application.Interfaces;

namespace MovieMvcProject.Application.Features.Comments.Events
{
    public class CommentChangedEventHandler : INotificationHandler<CommentChangedEvent>
    {
        private readonly IMovieRatingService _ratingService;

        public CommentChangedEventHandler(IMovieRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        public async Task Handle(CommentChangedEvent notification, CancellationToken cancellationToken)
        {
            await _ratingService.CalculateAndUpdateAverageRatingAsync(notification.MovieId);
        }
    }
}
