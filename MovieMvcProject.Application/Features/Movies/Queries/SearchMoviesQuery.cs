
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using System.Globalization;

namespace MovieMvcProject.Application.Features.Movies.Queries
{
    public class SearchMoviesQuery : IRequest<PagedResult<MovieDtoResponse>>
    {
        public string Query { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = CultureInfo.CurrentCulture.TwoLetterISOLanguageName ?? "tr";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10; 

        public SearchMoviesQuery(string query, string? languageCode = null, int pageNumber = 1, int pageSize = 10)
        {
            Query = !string.IsNullOrWhiteSpace(query) ? query.Trim() : throw new ArgumentException("Query boş olamaz.", nameof(query));
            LanguageCode = !string.IsNullOrEmpty(languageCode)
                ? languageCode.ToLowerInvariant()
                : CultureInfo.CurrentCulture.TwoLetterISOLanguageName ?? "tr";

            PageNumber = Math.Max(1, pageNumber); 
            PageSize = Math.Max(1, Math.Min(50, pageSize)); 
        }
    }
}