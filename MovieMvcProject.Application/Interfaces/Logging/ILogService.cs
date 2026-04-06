
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.LogDto;

namespace MovieMvcProject.Application.Interfaces.Logging
{
    public interface ILogService
    {
        Task<List<LogDto>> GetRecentLogsAsync(int count = 10);
        Task<PagedResult<LogDto>> GetPagedLogsAsync(LogFilterDto filter);
        Task<LogDto?> GetLogByIdAsync(int id);
        Task<Dictionary<string, int>> GetLogLevelCountsAsync(int lastHours = 24);
        Task<bool> ClearAllLogsAsync();
    }
}
