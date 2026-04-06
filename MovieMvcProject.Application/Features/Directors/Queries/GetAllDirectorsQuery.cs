using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Directors.Queries
{
    public record GetAllDirectorsQuery(int PageNumber = 1, int PageSize = 10, string? SearchTerm = null)
        : IRequest<PagedResult<DirectorLookupDto>>;
}
