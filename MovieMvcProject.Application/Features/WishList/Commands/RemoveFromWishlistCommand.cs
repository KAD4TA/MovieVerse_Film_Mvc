using MediatR;

namespace MovieMvcProject.Application.Features.WishList.Commands
{
    public class RemoveFromWishlistCommand : IRequest<bool>
    {
        public Guid MovieId { get; }
        public string UserId { get; }

        public RemoveFromWishlistCommand(Guid movieId, string userId)
        {
            MovieId = movieId;
            UserId = userId;
        }
    }
}
