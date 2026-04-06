
using MovieMvcProject.Domain.Enums;



namespace MovieMvcProject.Web.Models
{
    public class RegisterViewModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

        public bool RememberMe { get; set; } = false;
        public IFormFile? ProfileImageFile { get; set; } 
    }


}
