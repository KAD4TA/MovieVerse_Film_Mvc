using MovieMvcProject.Application.DTOs.Dashboard;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        


        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<Dictionary<DateTime, int>> GetDailyUserTrafficAsync(int days = 7);
        Task<Dictionary<DateTime, int>> GetDailyVisitorTrafficAsync(int days = 7);
        Task<List<GenrePercentageDto>> GetGenrePercentagesAsync();


        Task<List<TrendingMovieDtoResponse>> GetWeeklyTrendingMoviesAsync(string langCode, int limit = 10);


        Task<List<VisitorReportDto>> GetVisitorReportDataAsync(int days);
    }
}
