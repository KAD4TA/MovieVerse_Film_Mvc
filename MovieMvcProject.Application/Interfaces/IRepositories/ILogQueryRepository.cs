

using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.LogDto;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Interfaces.IRepositories
{
    public interface ILogQueryRepository
    {
        Task<List<LogEntry>> GetRecentLogsAsync(int count);
        Task<PagedResult<LogEntry>> GetPagedLogsAsync(LogFilterDto filter);

 
        Task<LogEntry?> GetLogByIdAsync(int id);

        Task<Dictionary<string, int>> GetLogLevelCountsAsync(int lastHours = 24);

      
        Task<bool> ClearAllLogsAsync();
    }
}
