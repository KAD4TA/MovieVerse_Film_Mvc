namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class DeleteCommentDtoResponse
    {
        public bool Success { get; set; } = true;  
        public required string Message { get; set; }
    }

}
