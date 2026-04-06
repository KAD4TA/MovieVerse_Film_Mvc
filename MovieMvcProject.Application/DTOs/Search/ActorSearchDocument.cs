namespace MovieMvcProject.Application.DTOs.Search
{
   

    public class ActorSearchDocument
    {
        public Guid ActorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string ProfilePath { get; set; } = string.Empty;
        public int? Height { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }

        
        public ActorSearchDocument() { }

        
        public ActorSearchDocument(Guid id, string name, string? photoUrl, string profilePath, int? height, DateTime? birthDate, string? birthPlace)
        {
            ActorId = id;
            Name = name;
            PhotoUrl = photoUrl;
            ProfilePath = profilePath;
            Height = height;
            BirthDate = birthDate;
            BirthPlace = birthPlace;
        }
    }


}
