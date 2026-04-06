using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.DTOs.Dashboard;
using MovieMvcProject.Application.Interfaces.Dashboard;
using MovieMvcProject.Application.Interfaces.Logging;
using MovieMvcProject.Web.Areas.Admin.Models;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    [Authorize(Roles = "admin,moderator")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogService _logService;

        public DashboardController(IDashboardService dashboardService, ILogService logService)
        {
            _dashboardService = dashboardService;
            _logService = logService;
        }

        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            var recentLogs = await _logService.GetRecentLogsAsync(3);

            var viewModel = new DashboardViewModel
            {
                Summary = summary,
                RecentLogs = recentLogs,
                GenrePercentages = summary.GenrePercentages ?? new List<GenrePercentageDto>()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyTraffic(int days = 7, string type = "users")
        {
            var trafficData = string.Equals(type, "visitors", StringComparison.OrdinalIgnoreCase)
                ? await _dashboardService.GetDailyVisitorTrafficAsync(days)
                : await _dashboardService.GetDailyUserTrafficAsync(days);

           
            var dateFormat = "dd.MM.yyyy";          
                                                    

            return Json(new
            {
                dates = trafficData.Keys
                    .OrderBy(d => d)                    
                    .Select(d => d.ToString(dateFormat))
                    .ToList(),
                counts = trafficData.Values.ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetVisitorReportData(int days = 7)
        {
           
            var reportData = await _dashboardService.GetVisitorReportDataAsync(days);

            
            return Json(reportData);
        }
    }
}
