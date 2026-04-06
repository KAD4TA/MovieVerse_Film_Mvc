using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Actors.Queries
{
    
    public record GetActorWithMoviesQuery(
        Guid ActorId,
        int PageNumber = 1,
        int PageSize = 10) : IRequest<ActorDetailDto>;
}
