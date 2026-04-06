
using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Movies.Commands
{
    public class CreateMovieCommand : IRequest<MovieDtoResponse>
    {
        public CreateMovieCommand(CreateMovieDto movieDto)
        {
            
            MovieDto = movieDto ?? throw new ArgumentNullException(nameof(movieDto));
        }
        public CreateMovieDto MovieDto { get; set; }
    }
}



