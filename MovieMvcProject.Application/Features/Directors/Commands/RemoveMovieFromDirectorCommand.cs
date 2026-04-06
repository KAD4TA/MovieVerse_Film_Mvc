using MediatR;

namespace MovieMvcProject.Application.Features.Directors.Commands
{
    public record RemoveMovieFromDirectorCommand(Guid DirectorId, Guid MovieId) : IRequest<bool>;
}
