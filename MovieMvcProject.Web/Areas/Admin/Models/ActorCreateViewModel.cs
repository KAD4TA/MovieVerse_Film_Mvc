using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class ActorCreateViewModel
    {

        [Required(ErrorMessage = "İsim zorunludur.")]
        [Display(Name = "İsim")]
        public required string Name { get; set; }

        [Display(Name = "Fotoğraf URL")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Doğum Yeri")]
        public string? BirthPlace { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(50, 250)]
        public int? Height { get; set; }
    }
}
