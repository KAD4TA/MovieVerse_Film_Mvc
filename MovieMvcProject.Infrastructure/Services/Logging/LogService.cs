
using AutoMapper;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.LogDto;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Application.Interfaces.Logging;

namespace MovieMvcProject.Infrastructure.Services.Logging
{
    public class LogService : ILogService
    {
        private readonly ILogQueryRepository _repository;
        private readonly IMapper _mapper;

        public LogService(ILogQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedResult<LogDto>> GetPagedLogsAsync(LogFilterDto filter)
        {
            var pagedEntries = await _repository.GetPagedLogsAsync(filter);

            
            var logDtos = _mapper.Map<IReadOnlyCollection<LogDto>>(pagedEntries.Items);

            return new PagedResult<LogDto>(
                logDtos,
                pagedEntries.TotalCount,
                pagedEntries.PageNumber,
                pagedEntries.PageSize
            );
        }

        public async Task<List<LogDto>> GetRecentLogsAsync(int count = 10)
        {
            var entries = await _repository.GetRecentLogsAsync(count);
            return _mapper.Map<List<LogDto>>(entries);
        }

        public async Task<LogDto?> GetLogByIdAsync(int id)
        {
            var entry = await _repository.GetLogByIdAsync(id);
            return entry != null ? _mapper.Map<LogDto>(entry) : null;
        }

        public async Task<Dictionary<string, int>> GetLogLevelCountsAsync(int lastHours = 24)
        {
            return await _repository.GetLogLevelCountsAsync(lastHours);
        }

        public async Task<bool> ClearAllLogsAsync()
        {
            return await _repository.ClearAllLogsAsync();
        }
    }
}

