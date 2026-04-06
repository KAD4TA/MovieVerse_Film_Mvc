using System.ComponentModel.DataAnnotations;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class MovieViewModel
    {
        public Guid MovieId { get; set; }

        [Display(Name = "Film Başlığı")]
        public string Title { get; set; } 

        [Display(Name = "Kategori")]
        public string Category { get; set; }

        [Display(Name = "Yıl")]
        public int Year { get; set; }

        [Display(Name = "IMDB Puanı")]
        public double Rating { get; set; }

        public string PosterUrl { get; set; }

        [Display(Name = "Slider'da Göster")]
        public bool IsOnSlider { get; set; }
    }
}
