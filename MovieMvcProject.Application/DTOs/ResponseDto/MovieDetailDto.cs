

using MovieMvcProject.Domain.Entities.EntityTranslations;

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class MovieDetailDto
    {
        public Guid MovieId { get; set; }
        
        public int Year { get; set; }
        public double Rating { get; set; }
        public required string Category { get; set; } 

        public required string Title { get; set; }
        public required string Description { get; set; }

        public required string PosterUrl { get; set; }
        public required string VideoUrl { get; set; }
        public int DurationInMinutes { get; set; } 
        public double MovieAvgReviewRate { get; set; }

        
        public List<MovieTranslation> Translations { get; set; } = new();

        public DirectorLookupDto Director { get; set; } = null!;
        public List<CommentDtoResponse> Comments { get; set; } = new();
        public List<ActorDtoResponse> Actors { get; set; } = new();
    }
}