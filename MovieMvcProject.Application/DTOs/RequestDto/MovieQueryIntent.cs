namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class MovieQueryIntent
    {
        public string? ActorName { get; set; }
        public string? DirectorName { get; set; }
        public string? Category { get; set; }
        public double? MinRating { get; set; }
        public int? MinYear { get; set; }
        public string? SemanticSearch { get; set; }
    }
}
