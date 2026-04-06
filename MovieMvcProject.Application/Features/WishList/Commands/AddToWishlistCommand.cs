using MediatR;

namespace MovieMvcProject.Application.Features.WishList.Commands
{
    public class AddToWishlistCommand : IRequest<bool>
    {
        public Guid MovieId { get; set; }
        public string UserId { get; set; }
    }
}
