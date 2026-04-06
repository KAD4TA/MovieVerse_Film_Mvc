using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Models
{
    public class ActorEditViewModel
    {
        public Guid ActorId { get; set; }

        [Required(ErrorMessage = "Oyuncu adı zorunludur.")]
        [Display(Name = "Oyuncu Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Avatar/Resim URL")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Doğum Yeri")]
        public string? BirthPlace { get; set; }

        [Display(Name = "Boy (cm)")]
        public int? Height { get; set; }

        [Display(Name = "Kilo (kg)")]


        
        public List<ActorMovieItemViewModel> Movies { get; set; } = new();
    }
}
