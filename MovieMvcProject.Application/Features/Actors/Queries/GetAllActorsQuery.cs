using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Actors.Queries
{
    
    public record GetAllActorsQuery(int PageNumber = 1, int PageSize = 10, string? SearchTerm = null)
    : IRequest<PagedResult<ActorListDto>>;
}
