using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Features.WishList.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Features.WishList.Handlers
{
    public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public AddToWishlistCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<bool> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
        {
            // Zaten ekli mi kontrolü
            var exists = await _context.Wishlists
                .AnyAsync(x => x.MovieId == request.MovieId && x.UserId == request.UserId);

            if (exists) return false;

            var wishlist = new Wishlist
            {
                MovieId = request.MovieId,
                UserId = request.UserId,
                AddedAt = DateTime.Now
            };

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
