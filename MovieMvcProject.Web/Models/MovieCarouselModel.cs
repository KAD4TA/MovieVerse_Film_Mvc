using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Web.Models
{
    public class MovieCarouselModel
    {
        public string HeaderKey { get; set; }
        public List<MovieDtoResponse> Movies { get; set; }
        public string SectionId { get; set; }
        public IStringLocalizer<MenuResource> MenuLocalizer { get; set; }
    }
}