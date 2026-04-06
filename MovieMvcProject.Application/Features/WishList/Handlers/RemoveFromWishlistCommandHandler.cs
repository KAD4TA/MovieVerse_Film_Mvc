using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Features.WishList.Commands;
using MovieMvcProject.Application.Interfaces;

namespace MovieMvcProject.Application.Features.WishList.Handlers
{
    public class RemoveFromWishlistCommandHandler
    : IRequestHandler<RemoveFromWishlistCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public RemoveFromWishlistCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
        {
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(x =>
                    x.MovieId == request.MovieId &&
                    x.UserId == request.UserId,
                    cancellationToken);

            if (wishlistItem == null)
                return false; 

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
