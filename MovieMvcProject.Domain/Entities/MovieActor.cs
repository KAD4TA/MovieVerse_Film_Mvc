
namespace MovieMvcProject.Domain.Entities
{
    public class MovieActor
    {
        
        public Guid MovieId { get; set; }
        public Movie Movie { get; set; }

        public Guid ActorId { get; set; }
        public Actor Actor { get; set; }

        public DateTime? BirthDate { get; set; } // Doğum Tarihi
        public string? BirthPlace { get; set; }  // Doğum Yeri
        public int? Height { get; set; }         // Boy (Örn: cm cinsinden 185)


        // EF Core için gerekli (composite key)
        public override bool Equals(object? obj) => obj is MovieActor ma && MovieId == ma.MovieId && ActorId == ma.ActorId;
        public override int GetHashCode() => HashCode.Combine(MovieId, ActorId);
    }
}