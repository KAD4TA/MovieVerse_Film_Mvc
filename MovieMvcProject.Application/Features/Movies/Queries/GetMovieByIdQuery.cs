
using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Movies.Queries
{
    public class GetMovieByIdQuery : IRequest<MovieDetailDto>
    {
        public GetMovieByIdQuery(Guid id)
        {                                       
            MovieId = id;
        }
        public Guid MovieId { get; set; }
    }


}
