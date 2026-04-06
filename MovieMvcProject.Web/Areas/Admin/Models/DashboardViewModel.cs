using MovieMvcProject.Application.DTOs.Dashboard;
using MovieMvcProject.Application.DTOs.LogDto;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        
        public required DashboardSummaryDto Summary { get; set; }

        public required List<LogDto> RecentLogs { get; set; }

        public required List<GenrePercentageDto> GenrePercentages { get; set; }
    }
}
