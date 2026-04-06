


using Microsoft.AspNetCore.Http;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class ProfileUpdateRequestDto
    {
        // Controller'dan gelen kullanıcı ID'si 
        public required string UserId { get; set; }

        public required string FullName { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? Email { get; set; }
        public string? ConfirmNewPassword { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

        // Dosya transferi (Service katmanında bu kullanılır)
        public IFormFile? ProfileImageFile { get; set; }

        // Kaydedilmiş dosyanın yolu (Controller tarafından doldurulur)
        public string? ProfileImagePath { get; set; }
    }
}