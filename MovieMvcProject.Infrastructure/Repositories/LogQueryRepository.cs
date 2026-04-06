

using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.LogDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Infrastructure.Repositories
{
    public class LogQueryRepository : ILogQueryRepository
    {
        private readonly IApplicationDbContext _context;

        public LogQueryRepository(IApplicationDbContext context)
        {
            _context = context;
        }

      
        public async Task<PagedResult<LogEntry>> GetPagedLogsAsync(LogFilterDto filter)
        {
            var query = _context.Logs.AsNoTracking().AsQueryable();

            // Log Seviyesi Filtresi
            if (!string.IsNullOrWhiteSpace(filter.LogLevel))
                query = query.Where(l => l.Level == filter.LogLevel);

            // Arama Filtresi
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(l => l.Message.Contains(filter.SearchTerm) || l.Properties.Contains(filter.SearchTerm));

            // --- TARİH FİLTRELERİ BURADA ---
            if (filter.StartDate.HasValue)
            {
                var start = filter.StartDate.Value.Date; // 00:00:00
                query = query.Where(l => l.TimeStamp >= start);
            }

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1); // 23:59:59
                query = query.Where(l => l.TimeStamp <= end);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.TimeStamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<LogEntry>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<List<LogEntry>> GetRecentLogsAsync(int count)
        {
            return await _context.Logs
                .AsNoTracking()
                .OrderByDescending(l => l.TimeStamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<LogEntry?> GetLogByIdAsync(int id)
        {
            return await _context.Logs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Dictionary<string, int>> GetLogLevelCountsAsync(int lastHours = 24)
        {
            var dateThreshold = DateTime.UtcNow.AddHours(-lastHours);
            return await _context.Logs
                .AsNoTracking()
                .Where(l => l.TimeStamp >= dateThreshold)
                .GroupBy(l => l.Level)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Level, x => x.Count);
        }

        public async Task<bool> ClearAllLogsAsync()
        {
            try
            {
                await _context.Logs.ExecuteDeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}