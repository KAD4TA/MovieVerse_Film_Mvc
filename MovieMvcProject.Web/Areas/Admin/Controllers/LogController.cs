using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.LogDto;
using MovieMvcProject.Application.Interfaces.Logging;
using MovieMvcProject.Web.Areas.Admin.Models;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
    

    [Area("Admin")]
    public class LogController : Controller
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<IActionResult> List([FromQuery] LogFilterDto filter)
        {
            filter ??= new LogFilterDto();
            filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;
            filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;

           
            var pagedEntityResult = await _logService.GetPagedLogsAsync(filter);

            
            var logDtos = pagedEntityResult.Items.Select(x => new LogDto
            {
                Id = x.Id,
                Message = x.Message,
                Level = x.Level,
                TimeStamp = x.TimeStamp, 
                Exception = x.Exception,
                Properties = x.Properties
            }).ToList();

            
            var pagedDtoResult = new PagedResult<LogDto>(
                logDtos,
                pagedEntityResult.TotalCount,
                pagedEntityResult.PageNumber,
                pagedEntityResult.PageSize
            );

            var viewModel = new LogListViewModel
            {
                Logs = pagedDtoResult,
                Filter = filter
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var log = await _logService.GetLogByIdAsync(id);
            if (log == null) return NotFound();

            return Json(new
            {
                exception = log.Exception,
                properties = log.Properties,
                message = log.Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearLogs()
        {
            var result = await _logService.ClearAllLogsAsync();
            if (result)
                TempData["SuccessMessage"] = "System logs have been cleared successfully.";
            else
                TempData["ErrorMessage"] = "An error occurred while clearing the logs.";

            return RedirectToAction(nameof(List));
        }
    }
}
