using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Domain.Entities
{
    public static class PageTypes
    {
        public const string MovieDetail = "MovieDetail";
        
    }

    public class MovieVisitLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MovieId { get; set; }
        public virtual Movie? Movie { get; set; }

        public string? UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        
        public DateTime VisitedAt { get; set; } = DateTime.Now;
        public string PageType { get; set; } = PageTypes.MovieDetail;
    }
}
