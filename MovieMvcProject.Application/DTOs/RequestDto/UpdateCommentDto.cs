namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class UpdateCommentDto
    {
        public required Guid CommentId { get; set; }        // Güncellenecek yorumun Id'si
        public required string Content { get; set; } // Yeni yorum metni

        public required Guid MovieId { get; set; }
        public int? MovieReview { get; set; }        // Opsiyonel puanlama
    }
}


