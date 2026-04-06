namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class DeleteMovieDtoResponse
    {
        public bool Success { get; set; } = true;   
        public required string Message { get; set; }
    }
}
