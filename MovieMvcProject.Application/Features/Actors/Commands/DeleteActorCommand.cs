using MediatR;

namespace MovieMvcProject.Application.Features.Actors.Commands
{
    public record DeleteActorCommand(Guid ActorId) : IRequest<bool>;
}
