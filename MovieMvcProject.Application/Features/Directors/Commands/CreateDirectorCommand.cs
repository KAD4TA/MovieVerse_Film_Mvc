using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;

namespace MovieMvcProject.Application.Features.Directors.Commands
{
    public record CreateDirectorCommand(CreateDirectorDto Dto) : IRequest<Guid>;
}
