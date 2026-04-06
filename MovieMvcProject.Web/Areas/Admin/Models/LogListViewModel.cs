using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.LogDto;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class LogListViewModel
    {
        public PagedResult<LogDto> Logs { get; set; } = PagedResult<LogDto>.Empty(1, 20);
        public LogFilterDto Filter { get; set; } = new LogFilterDto();
    }
}
