using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Actors.Queries
{
    public record GetActorForEditQuery(Guid ActorId) : IRequest<ActorEditDto?>;
}
