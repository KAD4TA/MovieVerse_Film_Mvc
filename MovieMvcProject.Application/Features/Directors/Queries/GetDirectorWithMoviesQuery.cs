using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Directors.Queries
{

    public record GetDirectorWithMoviesQuery(Guid DirectorId, int PageNumber = 1, int PageSize = 10) : IRequest<DirectorDetailDto>;
}
