namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class ActorMovieDto
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? PosterUrl { get; set; }
    }
}
