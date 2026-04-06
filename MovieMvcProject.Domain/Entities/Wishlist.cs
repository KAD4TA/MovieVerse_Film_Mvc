using MovieMvcProject.Domain.Identity;

namespace MovieMvcProject.Domain.Entities
{
    public class Wishlist
    {
        public Guid MovieId { get; set; }
        public string UserId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now; 

        // Navigation Properties
        public virtual Movie Movie { get; set; }
        public virtual AppUser User { get; set; }
    }
}
