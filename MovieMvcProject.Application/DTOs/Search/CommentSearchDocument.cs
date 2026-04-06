namespace MovieMvcProject.Application.DTOs.Search
{
    

    public class CommentSearchDocument
    {
        public Guid CommentId { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public string UserProfileImageUrl { get; set; }
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; } 
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public int MovieReview { get; set; } 
    }
}
