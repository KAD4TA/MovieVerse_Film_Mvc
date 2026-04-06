using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;

namespace MovieMvcProject.Application.Features.Actors.Commands
{
    
    public record CreateActorCommand(CreateActorDto Dto) : IRequest<Guid>;
}