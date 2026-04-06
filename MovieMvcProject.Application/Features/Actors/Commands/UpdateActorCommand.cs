using MediatR;

namespace MovieMvcProject.Application.Features.Actors.Commands
{
    

    public record UpdateActorCommand : IRequest<bool>
    {
        public Guid ActorId { get; init; }
        public required string Name { get; init; }
        public string? AvatarUrl { get; init; }
        public DateTime? BirthDate { get; init; }
        public string? BirthPlace { get; init; }
        public int? Height { get; init; }
    }
}
