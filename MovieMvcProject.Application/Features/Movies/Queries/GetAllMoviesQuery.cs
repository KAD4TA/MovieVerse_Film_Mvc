
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;


namespace MovieMvcProject.Application.Features.Movies.Queries
{
    public class GetAllMoviesQuery : IRequest<PagedResult<MovieDtoResponse>>
    {
        
        public string LanguageCode { get; set; } = "tr";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        
        public string? SearchTerm { get; set; }

       
        public GetAllMoviesQuery() { }

      
        public GetAllMoviesQuery(int pageNumber, int pageSize, string languageCode, string? searchTerm = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            LanguageCode = languageCode;
            SearchTerm = searchTerm;
        }

        
        public GetAllMoviesQuery(int pageNumber, int pageSize, string languageCode)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            LanguageCode = languageCode;
        }
    }
}
