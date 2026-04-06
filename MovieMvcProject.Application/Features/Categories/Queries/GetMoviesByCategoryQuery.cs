using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Categories.Queries
{
    public record GetMoviesByCategoryQuery(
    string LanguageCode,
    string CategoryName,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<MovieDtoResponse>>;
}
