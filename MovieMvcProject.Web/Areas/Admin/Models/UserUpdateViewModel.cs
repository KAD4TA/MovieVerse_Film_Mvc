using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class UserUpdateViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }


        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Profil Resmi")]
        public IFormFile? ProfileImage { get; set; }

        // --- Ek Profil Bilgileri ---

        [Display(Name = "Cinsiyet")]
        public string? Gender { get; set; } // Enum ise Dropdown ile doldurulur

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Instagram Profili")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string? InstagramUrl { get; set; }

        [Display(Name = "Twitter (X) Profili")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string? TwitterUrl { get; set; }
    }
}
