using MovieMvcProject.Application.Commons;

namespace MovieMvcProject.Web.Models
{
    public class UserPageViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

       
        public PagedResult<WishlistViewModel> Wishlists { get; set; }
            = new PagedResult<WishlistViewModel>(new List<WishlistViewModel>().AsReadOnly(), 0, 1, 12);

        public bool IsOwnProfile { get; set; }
    }
}
