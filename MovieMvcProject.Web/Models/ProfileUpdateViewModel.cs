using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Web.Models
{
    public class ProfileUpdateViewModel
    {
        
        public required string UserId { get; set; }

        public required string FullName { get; set; }

        
        public string? ProfileImage { get; set; }

       
        public required string Email { get; set; }

        
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

        
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

       
        public IFormFile? ProfileImageFile { get; set; }
    }
}