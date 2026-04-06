using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Domain.Entities
{
    public class Actor
    {
        [Key]
        public Guid ActorId { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }               // Oyuncu adı
        public string AvatarUrl { get; set; } = "profile-images/default-profile.png";

        public DateTime? BirthDate { get; set; } // Doğum Tarihi
        public string? BirthPlace { get; set; }  // Doğum Yeri
        public int? Height { get; set; }         // Boy (Örn: cm cinsinden 185)

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
