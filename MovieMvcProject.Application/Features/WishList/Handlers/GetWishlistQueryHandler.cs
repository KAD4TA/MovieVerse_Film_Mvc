
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.WishList.Queries;
using MovieMvcProject.Application.Interfaces;

namespace MovieMvcProject.Application.Features.WishList.Handlers
{
    

    public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, PagedResult<WishlistDtoResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetWishlistQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<WishlistDtoResponse>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Wishlists
                .Where(x => x.UserId == request.UserId)   
                .Include(x => x.Movie)
                    .ThenInclude(m => m.Translations)
                .OrderByDescending(x => x.AddedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<WishlistDtoResponse>>(items);

            return new PagedResult<WishlistDtoResponse>(
                dtos.AsReadOnly(),
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
