namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class ActorDtoResponse
    {
        public Guid ActorId { get; set; }
        public required string Name { get; set; }
        public required string AvatarUrl { get; set; }
        public int? Height { get; set; }
        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
    }

}
