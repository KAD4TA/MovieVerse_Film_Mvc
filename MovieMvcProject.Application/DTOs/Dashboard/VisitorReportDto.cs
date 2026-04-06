namespace MovieMvcProject.Application.DTOs.Dashboard
{
    public class VisitorReportDto
    {
        public DateTime VisitedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public Guid MovieId { get; set; }
        public string? MovieTitle { get; set; }
    }
}
