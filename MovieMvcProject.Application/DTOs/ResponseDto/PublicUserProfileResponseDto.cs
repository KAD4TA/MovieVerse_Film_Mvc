using MovieMvcProject.Application.Commons;

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class PublicUserProfileResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

        public PagedResult<WishlistDtoResponse> Wishlists { get; set; }
            = new PagedResult<WishlistDtoResponse>(new List<WishlistDtoResponse>().AsReadOnly(), 0, 1, 12);
    }
}
