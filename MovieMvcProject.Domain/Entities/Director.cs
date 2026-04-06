
using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Domain.Entities
{
    public class Director
    {
        // Birincil Anahtar
        [Key]
        public Guid DirectorId { get; set; } = Guid.NewGuid();

        // Yönetmen adý (Zorunlu alan)
        public required string Name { get; set; }

        public DateTime? BirthDate { get; set; } // Doðum Tarihi
        public string? BirthPlace { get; set; }  // Doðum Yeri
        public int? Height { get; set; }

        // Yönetmen fotoðraf URL'si
        public string PhotoUrl { get; set; } = "/profile-images/default-profile.png"; // Varsayýlan deðer atanabilir.

        // Ýliþkiler: Bir yönetmen birden fazla film yönetebilir.
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<Movie> DirectedMovies { get; set; } = new List<Movie>();
    }
}