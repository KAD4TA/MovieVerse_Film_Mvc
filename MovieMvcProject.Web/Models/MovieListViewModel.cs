using MovieMvcProject.Application.DTOs.ResponseDto;


namespace MovieMvcProject.Web.Models
{
    public class MovieListViewModel
    {
        public List<MovieDtoResponse> Movies { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public string LanguageCode { get; set; } = "tr";
    }

}
