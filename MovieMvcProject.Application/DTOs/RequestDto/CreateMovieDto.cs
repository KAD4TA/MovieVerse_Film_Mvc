namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class CreateMovieDto
    {
        public required int Year { get; set; }
        public required double Rating { get; set; }
        public required string Category { get; set; }

        public required int DurationInMinutes { get; set; }
        public required string EnglishTitle { get; set; }

        public required string EnglishDescription { get; set; }
        public required string TurkishTitle { get; set; }

        public required string TurkishDescription { get; set; }

        public string? PosterUrl { get; set; }
        public required string VideoUrl { get; set; }


        public Guid? ExistingDirectorId { get; set; } // Var olan yönetmen seçildiyse ID'si
        public CreateDirectorDto? NewDirector { get; set; } // Yeni yönetmen eklenecekse bilgileri   
       

        public List<Guid> ExistingActorIds { get; set; } = new List<Guid>();
        public List<CreateActorDto> Actors { get; set; } = new();

    }
}
