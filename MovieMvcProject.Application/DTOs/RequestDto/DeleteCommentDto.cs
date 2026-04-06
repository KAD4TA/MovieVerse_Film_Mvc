namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class DeleteCommentDto
    {
        public required Guid CommentId { get; set; }   // Silinecek yorumun Id'si
        public required Guid MovieId { get; set; }
    }

}
