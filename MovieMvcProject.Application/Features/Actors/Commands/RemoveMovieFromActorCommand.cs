using MediatR;

namespace MovieMvcProject.Application.Features.Actors.Commands
{
    public record RemoveMovieFromActorCommand(Guid ActorId, Guid MovieId) : IRequest<bool>;
}
