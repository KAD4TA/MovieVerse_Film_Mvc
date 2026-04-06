
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieMvcProject.Domain.Entities
{
    public class Comment
    {
        [Key]
        public Guid CommentId { get; set; } = Guid.NewGuid();

        
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // --- İLİŞKİLER ---

        // Foreign Key'ler (Veritabanı için zorunlu ID'ler)
        public required Guid MovieId { get; set; }
        public required string UserId { get; set; }

        
        [ForeignKey("MovieId")]
        public virtual Movie? Movie { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        public int? MovieReview { get; set; }

        public Guid? ParentId { get; set; }
        public Comment? Parent { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public CommentStatus Status { get; set; } = CommentStatus.Pending;
    }
}
