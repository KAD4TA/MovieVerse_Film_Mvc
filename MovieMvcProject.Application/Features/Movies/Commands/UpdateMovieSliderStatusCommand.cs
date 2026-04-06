using MediatR;


namespace MovieMvcProject.Application.Features.Movies.Commands
{
    public class UpdateMovieSliderStatusCommand : IRequest<bool>
    {
        public Guid MovieId { get; set; }
        public bool IsOnSlider { get; set; }

        public UpdateMovieSliderStatusCommand(Guid movieId, bool isOnSlider)
        {
            MovieId = movieId;
            IsOnSlider = isOnSlider;
        }
    }
}
