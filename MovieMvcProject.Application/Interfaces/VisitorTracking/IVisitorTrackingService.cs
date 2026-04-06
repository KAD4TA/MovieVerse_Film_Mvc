namespace MovieMvcProject.Application.Interfaces.VisitorTracking
{
    public interface IVisitorTrackingService
    {
        Task TrackMovieVisitAsync(Guid movieId, string? userId, string ipAddress, string userAgent);
    }
}
