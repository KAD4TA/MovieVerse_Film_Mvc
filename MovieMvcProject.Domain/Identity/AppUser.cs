using Microsoft.AspNetCore.Identity;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Domain.Identity
{
    public class AppUser : IdentityUser
    {


        // Profil bilgileri
        public required string FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }

        


        // Sosyal medya
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

        // Kullanıcı aktiviteleri
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

    }
}
