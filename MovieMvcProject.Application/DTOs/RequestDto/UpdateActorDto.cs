namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class UpdateActorDto
    {
        public Guid ActorId { get; set; }
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public int? Height { get; set; }
        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
