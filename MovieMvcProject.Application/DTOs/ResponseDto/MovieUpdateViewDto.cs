namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class MovieUpdateViewDto
    {
        public Guid MovieId { get; set; }
        public int Year { get; set; }
        public double Rating { get; set; }
        public string Category { get; set; } 
        public int DurationInMinutes { get; set; }
        public string PosterUrl { get; set; }
        public string VideoUrl { get; set; }

       
        public string TurkishTitle { get; set; } = string.Empty;
        public string EnglishTitle { get; set; } = string.Empty;
        public string TurkishDescription { get; set; } = string.Empty;
        public string EnglishDescription { get; set; } = string.Empty;

        public DirectorLookupDto Director { get; set; } = null!;
        public List<ActorDtoResponse> Actors { get; set; } = new();
    }
}
