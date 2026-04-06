namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class WishlistDtoResponse
    {
        public MovieDtoResponse Movie { get; set; } 
        public DateTime AddedAt { get; set; }
    }
}
