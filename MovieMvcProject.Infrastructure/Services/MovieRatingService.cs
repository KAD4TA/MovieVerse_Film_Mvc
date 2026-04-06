

using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Domain.Enums;

public class MovieRatingService : IMovieRatingService
{
    private readonly IApplicationDbContext _context;

    public MovieRatingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CalculateAndUpdateAverageRatingAsync(Guid movieId)
    {
        var avg = await _context.Comments
            .Where(c => c.MovieId == movieId
                     && c.MovieReview.HasValue
                     && c.MovieReview > 0
                     && c.Status == CommentStatus.Approved)
            .AverageAsync(c => (double?)c.MovieReview) ?? 0.0;

        var roundedAvg = Math.Round(avg, 1);

        var movie = await _context.Movies.FindAsync(movieId);
        if (movie != null)
        {
            movie.MovieAvgReviewRate = roundedAvg;
            await _context.SaveChangesAsync();
        }
    }
}