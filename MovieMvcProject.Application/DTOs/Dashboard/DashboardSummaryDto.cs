namespace MovieMvcProject.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }

        public int TotalVisitors { get; set; }
        public int TotalMovies { get; set; }
        public string? LatestUserName { get; set; }

        public string? LatestUserId { get; set; }

        public string? LatestUserNamePhoto { get; set; }
        public string? LatestCommentText { get; set; }
        
        public List<string> TrafficDates { get; set; } = new();
        public List<int> TrafficCounts { get; set; } = new();
        public List<GenrePercentageDto> GenrePercentages { get; set; } = new();
    }
}
