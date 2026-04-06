using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Movies.Queries
{
    public record GetMovieForUpdateQuery(Guid MovieId) : IRequest<MovieUpdateViewDto>;
}
