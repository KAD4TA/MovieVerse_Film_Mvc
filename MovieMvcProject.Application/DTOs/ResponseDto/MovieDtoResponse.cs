namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class MovieDtoResponse
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; } = string.Empty;

        public string PosterUrl { get; set; } = "/images/no-poster.jpg";
        public int Year { get; set; }
        public double Rating { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsOnSlider { get; set; }


    }

}
