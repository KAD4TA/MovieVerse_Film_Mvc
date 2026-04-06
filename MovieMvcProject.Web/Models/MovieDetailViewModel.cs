
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Domain.Enums; 

namespace MovieMvcProject.Web.Models
{
    public class MovieDetailViewModel
    {
        public MovieDetailData MovieDetail { get; set; } = new();
        public CreateCommentDto NewComment { get; set; } = new()
        {
            Content = string.Empty,
            MovieId = Guid.Empty
        };
        public List<CommentItem> Comments { get; set; } = new();
        public List<ActorItem> Actors { get; set; } = new();

        public class MovieDetailData
        {
            public Guid MovieId { get; set; }
            public string Title { get; set; } = string.Empty;
            public int Year { get; set; }
            public double Rating { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string PosterUrl { get; set; } = string.Empty;
            public string VideoUrl { get; set; } = string.Empty;
            public double MovieAvgReviewRate { get; set; }
            public Guid DirectorId { get; set; }
            public string Director { get; set; } = string.Empty;
            public string DirectorPhotoUrl { get; set; } = string.Empty;
            public int DurationInMinutes { get; set; } = 0;
        }

        public class CommentItem
        {
            public Guid CommentId { get; set; }
            public Guid MovieId { get; set; }

            
            public string UserId { get; set; }

            public string Content { get; set; } = string.Empty;
            public DateTime CommentDate { get; set; }
            public int? ReviewScore { get; set; }

         
            public CommentStatus Status { get; set; }

            public string UserName { get; set; } = string.Empty;
            public string UserProfilePictureUrl { get; set; } = "/profile-images/default-profile.png";

            public decimal DisplayRating => (decimal)(ReviewScore ?? 0);
        }

        public class ActorItem
        {
            public Guid ActorId { get; set; }
            public string ActorName { get; set; } = string.Empty;
            public string ProfilePictureUrl { get; set; } = "/profile-images/default-profile.png";
            public string Role { get; set; } = "Oyuncu";
        }
    }
}