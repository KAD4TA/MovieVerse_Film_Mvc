using MediatR;

namespace MovieMvcProject.Application.Features.Directors.Commands
{
    public record UpdateDirectorCommand : IRequest<bool>
    {
        public Guid DirectorId { get; init; }
        public required string Name { get; init; }
        public string? PhotoUrl { get; init; }
        public DateTime? BirthDate { get; init; }
        public string? BirthPlace { get; init; }
        public int? Height { get; init; }
    }
}
