using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.VisitorTracking;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Infrastructure.Services.VisitorTracking
{
    public class VisitorTrackingService : IVisitorTrackingService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<VisitorTrackingService> _logger;

        public VisitorTrackingService(IApplicationDbContext context, ILogger<VisitorTrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task TrackMovieVisitAsync(Guid movieId, string? userId, string ipAddress, string userAgent)
        {
            try
            {
                var visitLog = new MovieVisitLog
                {
                    MovieId = movieId,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    VisitedAt = DateTime.UtcNow,
                    PageType = PageTypes.MovieDetail
                };

                _context.MovieVisitLogs.Add(visitLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Ziyaretçi kaydedildi. MovieId: {MovieId} | IP: {IP}", movieId, ipAddress);
            }
            catch (Exception ex)
            {
                // Kullanıcı film detayını görmeye devam etmeli, log hatası sayfayı patlatmamalı.
                _logger.LogError(ex, " Ziyaretçi kayıt hatası. MovieId: {MovieId}", movieId);
            }
        }
    }
}
