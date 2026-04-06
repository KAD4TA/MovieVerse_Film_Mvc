

using MovieMvcProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Models
{
    public class SettingsPageViewModel
    {
        
        public string UserId { get; set; } = string.Empty;

        
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty; 

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public Gender Gender { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? ProfileImage { get; set; }

        public string? CurrentPassword { get; set; }

  
        public string? NewPassword { get; set; }


        public string? ConfirmNewPassword { get; set; }

        public IFormFile? ProfileImageFile { get; set; }

       
        public bool IsDarkModeEnabled { get; set; }
        public bool IsEmailNotificationEnabled { get; set; }

      
        public string ActiveTab { get; set; } = "profile";


    }
}
