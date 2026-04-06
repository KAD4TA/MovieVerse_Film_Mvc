namespace MovieMvcProject.Domain.Entities.EntityTranslations
{
    public class MovieTranslation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MovieId { get; set; }
        public string LanguageCode { get; set; } = "tr"; 
        public required string Title { get; set; }
        public required string Description { get; set; }

        public Movie? Movie { get; set; }
    }

}
