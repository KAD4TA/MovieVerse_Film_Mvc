using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class CreateDirectorDto
    {
        

        [Required(ErrorMessage = "Yönetmen adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Yönetmen adı 2-100 karakter arasında olmalıdır.")]
        public required string Name { get; set; } = string.Empty;

        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [StringLength(500, ErrorMessage = "Fotoğraf URL'si çok uzun.")]
        public string? PhotoUrl { get; set; } 

        public DateTime? BirthDate { get; set; } 
        public string? BirthPlace { get; set; }  
        public int? Height { get; set; }
    }
}
