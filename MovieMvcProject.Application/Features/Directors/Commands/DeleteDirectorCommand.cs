using MediatR;

namespace MovieMvcProject.Application.Features.Directors.Commands
{
    public record DeleteDirectorCommand(Guid DirectorId) : IRequest<bool>;
}
