using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Web.Models
{
    public class DirectorDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;

        
        public DateTime? BirthDate { get; set; }
        public string FormattedBirthDate => BirthDate?.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")) ?? "—";
        public string? AgeDisplay => BirthDate.HasValue
            ? $"{DateTime.Now.Year - BirthDate.Value.Year}"
            : "—";

        public string? BirthPlace { get; set; }
        public int? Height { get; set; }
        public string FormattedHeight => Height.HasValue ? $"{Height} cm" : "—";

        // Filmografisi
        public PagedResult<MovieDtoResponse> Movies { get; set; } = PagedResult<MovieDtoResponse>.Empty(1, 12);

        // View helper'lar
        public string PageTitle => $"{Name} | Yönetmen";
       
    }
}
