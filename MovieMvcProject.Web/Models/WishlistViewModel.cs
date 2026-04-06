using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Web.Models
{
    public class WishlistViewModel
    {
        public MovieDtoResponse Movie { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
