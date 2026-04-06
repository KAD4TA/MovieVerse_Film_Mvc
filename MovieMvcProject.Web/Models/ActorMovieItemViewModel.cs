namespace MovieMvcProject.Web.Models
{
    public class ActorMovieItemViewModel
    {
        public Guid MovieId { get; set; }

        public string? Title { get; set; }

        public string? Year { get; set; }
        public string? PosterUrl { get; set; }
    }
}
