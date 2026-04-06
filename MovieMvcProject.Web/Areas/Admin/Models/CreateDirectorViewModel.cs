using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class CreateDirectorViewModel
    {
        [Required(ErrorMessage = "İsim zorunludur.")]
        [Display(Name = "İsim")]
        public string Name { get; set; } = string.Empty; 

        [Display(Name = "Fotoğraf URL")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Doğum Yeri")]
        public string? BirthPlace { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(50, 250, ErrorMessage = "Boy 50-250 cm arası olmalıdır.")]
        public int? Height { get; set; }
    }
}
