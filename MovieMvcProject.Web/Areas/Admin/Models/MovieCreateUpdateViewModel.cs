using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class MovieCreateUpdateViewModel
    {
        // Update işleminde dolu gelir, Create işleminde boştur.
        public Guid? MovieId { get; set; }

        // --- Temel Bilgiler ---

        [Required(ErrorMessage = "Yapım yılı zorunludur.")]
        [Display(Name = "Yapım Yılı")]
        [Range(1900, 2100, ErrorMessage = "Geçerli bir yıl giriniz.")]
        public int Year { get; set; }

        // Film Süresi
        [Required(ErrorMessage = "Film süresi zorunludur.")]
        [Display(Name = "Film Süresi (Dakika)")]
        [Range(1, 600, ErrorMessage = "Film süresi 1 ile 600 dakika arasında olmalıdır.")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Puan zorunludur.")]
        [Display(Name = "Puan (0-10)")]
        [Range(0, 10, ErrorMessage = "Puan 0 ile 10 arasında olmalıdır.")]
        public double Rating { get; set; }

        [Required(ErrorMessage = "Kategori zorunludur.")]
        [Display(Name = "Kategori")]
        public Category Category { get; set; }

        // --- Dil Seçenekleri ---

        [Required(ErrorMessage = "İngilizce Başlık zorunludur.")]
        [Display(Name = "Başlık (İngilizce)")]
        public string EnglishTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "İngilizce Açıklama zorunludur.")]
        [Display(Name = "Açıklama (İngilizce)")]
        [DataType(DataType.MultilineText)]
        public string EnglishDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Türkçe Başlık zorunludur.")]
        [Display(Name = "Başlık (Türkçe)")]
        public string TurkishTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Türkçe Açıklama zorunludur.")]
        [Display(Name = "Açıklama (Türkçe)")]
        [DataType(DataType.MultilineText)]
        public string TurkishDescription { get; set; } = string.Empty;




        public string? PosterUrl { get; set; }

        [Required(ErrorMessage = "Video URL zorunludur.")]
        [Display(Name = "Video / Fragman URL")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string VideoUrl { get; set; } = string.Empty;

        // --- Yönetmen (Arama ve Ekleme İçin) ---

        // Arama sonucundan seçilen yönetmenin ID'si buraya atanır.
        [Display(Name = "Yönetmen")]
        public Guid? ExistingDirectorId { get; set; }

        // Seçilen veya yeni eklenen yönetmenin adını View'da göstermek için
        public string? ExistingDirectorName { get; set; }


        public string? DirectorName { get; set; }


        public string? DirectorPhotoUrl { get; set; }


        [Display(Name = "Doğum Yeri")]
        public string? DirectorBirthPlace { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arası olmalıdır.")]
        public int? DirectorHeightCm { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DirectorBirthDate { get; set; }

        // --- Oyuncular ---
        public List<ActorViewModel> Actors { get; set; } = new List<ActorViewModel>();
    }


}













