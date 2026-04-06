


namespace MovieMvcProject.Application.Interfaces
{
    public interface IMovieRatingService
    {
        
       
        Task CalculateAndUpdateAverageRatingAsync(Guid movieId);
    }
}
