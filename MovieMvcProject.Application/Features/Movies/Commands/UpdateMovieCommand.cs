

using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Movies.Commands;

public class UpdateMovieCommand : IRequest<MovieDtoResponse>
{
    public UpdateMovieCommand(UpdateMovieDto movieDto)
    {
        MovieDto = movieDto;
    }

    public UpdateMovieDto MovieDto { get; }
}

