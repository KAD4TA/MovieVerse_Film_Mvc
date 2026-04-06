namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class CreateCommentDto
    {
        public required string Content { get; set; }   // Yorum metni
        public required Guid MovieId { get; set; }     // Hangi filme yapıldığı
        public int? MovieReview { get; set; }          // Opsiyonel puanlama

        public Guid? ParentId { get; set; }
    }

}
