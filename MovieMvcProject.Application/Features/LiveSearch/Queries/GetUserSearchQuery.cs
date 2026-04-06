using MediatR;
using MovieMvcProject.Application.DTOs.LiveSearch;

namespace MovieMvcProject.Application.Features.LiveSearch.Queries
{
    public record GetUserSearchQuery(string Query, int PageSize) : IRequest<List<LiveSearchResultDto>>;
}
