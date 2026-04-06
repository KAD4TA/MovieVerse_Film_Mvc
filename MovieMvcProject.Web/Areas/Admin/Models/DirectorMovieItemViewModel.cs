namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class DirectorMovieItemViewModel
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? PosterUrl { get; set; }
    }
}
