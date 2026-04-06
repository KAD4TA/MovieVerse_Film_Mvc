namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class ActorEditDto
    {
        public Guid ActorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public int? Height { get; set; }

        public List<ActorMovieDto> Movies { get; set; } = new();
    }
}
