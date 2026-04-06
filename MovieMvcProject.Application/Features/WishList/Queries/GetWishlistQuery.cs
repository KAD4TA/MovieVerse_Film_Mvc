using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.WishList.Queries
{
    public record GetWishlistQuery(
        string UserId,
        int PageNumber = 1,
        int PageSize = 12)
        : IRequest<PagedResult<WishlistDtoResponse>>;
}
