
using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Movies.Commands
{
    public class DeleteMovieCommand : IRequest<DeleteMovieDtoResponse>
    {
        public DeleteMovieCommand(DeleteMovieDto movieDto)
        {
            MovieDto = movieDto;
        }
        public DeleteMovieDto MovieDto { get; set; }
    }
}
