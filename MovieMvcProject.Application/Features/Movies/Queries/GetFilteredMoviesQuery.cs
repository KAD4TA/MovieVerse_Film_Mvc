
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;


namespace MovieMvcProject.Application.Features.Movies.Queries
{
    public class GetFilteredMoviesQuery : IRequest<PagedResult<MovieDtoResponse>>
    {
        public string? Title { get; set; }
        public Guid? ActorId { get; set; }
        public double? MinRating { get; set; }
        public string? LanguageCode { get; set; } = "tr";
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = false;

        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
