using MediatR;

namespace MovieMvcProject.Application.Features.Comments.Events
{
    public class CommentChangedEvent : INotification
    {
        public Guid MovieId { get; }

        public CommentChangedEvent(Guid movieId)
        {
            MovieId = movieId;
        }
    }

}
