
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.DTOs.Dashboard;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Dashboard;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Infrastructure.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardService(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Dictionary<DateTime, int>> GetDailyUserTrafficAsync(int days = 7)
        {
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-days + 1);
            var data = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);
            return FillMissingDays(startDate, endDate, data);
        }

        
        public async Task<Dictionary<DateTime, int>> GetDailyVisitorTrafficAsync(int days = 7)
        {
            
            var endDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
            var startDate = endDate.Date.AddDays(-days + 1);

            var data = await _context.MovieVisitLogs
                .Where(v => v.VisitedAt >= startDate && v.VisitedAt <= endDate && v.PageType == PageTypes.MovieDetail)
                .GroupBy(v => v.VisitedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            return FillMissingDays(startDate.Date, endDate.Date, data);
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var trafficDict = await GetDailyUserTrafficAsync(7);
            var genreData = await GetGenrePercentagesAsync();

            var latestUser = await _context.Users.OrderByDescending(u => u.CreatedAt).FirstOrDefaultAsync();
            var latestComment = await _context.Comments
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            return new DashboardSummaryDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalMovies = await _context.Movies.CountAsync(),
                
                TotalVisitors = await _context.MovieVisitLogs.CountAsync(v => v.PageType == "MovieDetail"),

                LatestUserName = latestUser?.FullName ?? "Yok",
                LatestUserId = latestUser?.Id,
                LatestUserNamePhoto = latestUser?.ProfileImageUrl ?? "/profile-images/default-profile.png",
                LatestCommentText = latestComment?.Content ?? "Henüz yorum yok",
                TrafficDates = trafficDict.Keys.Select(d => d.ToString("dd.MM.yyyy HH:mm")).ToList(),
                TrafficCounts = trafficDict.Values.ToList(),
                GenrePercentages = genreData
            };
        }


        public async Task<List<TrendingMovieDtoResponse>> GetWeeklyTrendingMoviesAsync(string langCode, int limit = 10)
        {
            // 1. Son 7 günün tarihini belirle
            var startDate = DateTime.UtcNow.Date.AddDays(-7);

            // 2. Log tablosundan en çok izlenen film ID'lerini ve sayılarını alma
            var trendingLogs = await _context.MovieVisitLogs
                .AsNoTracking()
                .Where(v => v.VisitedAt >= startDate && v.PageType == PageTypes.MovieDetail)
                .GroupBy(v => v.MovieId)
                .OrderByDescending(g => g.Count())
                .Take(limit)
                .Select(g => new { MovieId = g.Key, ViewCount = g.Count() })
                .ToListAsync();

            if (!trendingLogs.Any()) return new List<TrendingMovieDtoResponse>();

            var movieIds = trendingLogs.Select(x => x.MovieId).ToList();

            // 3. Filmleri ve Çevirilerini veritabanından çekme
            var movies = await _context.Movies
                .Include(m => m.Translations)
                .Where(m => movieIds.Contains(m.MovieId))
                .ToListAsync();

            
            var dtos = _mapper.Map<List<TrendingMovieDtoResponse>>(movies, opt =>
            {
                opt.Items["LanguageCode"] = langCode;
            });

            
            foreach (var dto in dtos)
            {
                var logData = trendingLogs.FirstOrDefault(x => x.MovieId == dto.MovieId);
                if (logData != null)
                {
                    dto.WeeklyViewCount = logData.ViewCount;
                }
            }

            
            return dtos.OrderByDescending(x => x.WeeklyViewCount).ToList();
        }
    

        public async Task<List<GenrePercentageDto>> GetGenrePercentagesAsync()
        {
            return await _context.Movies
                .GroupBy(m => m.Category)
                .Select(g => new GenrePercentageDto
                {
                    Name = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();
        }

        private static Dictionary<DateTime, int> FillMissingDays(DateTime start, DateTime end, Dictionary<DateTime, int> data)
        {
            var result = new Dictionary<DateTime, int>();
            for (var dt = start; dt <= end; dt = dt.AddDays(1))
            {
                result[dt] = data.GetValueOrDefault(dt, 0);
            }
            return result;
        }



      
        public async Task<List<VisitorReportDto>> GetVisitorReportDataAsync(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var logs = await _context.MovieVisitLogs
                .AsNoTracking()  
                .Include(v => v.Movie)
                    .ThenInclude(m => m.Translations)
                .Where(v => v.VisitedAt >= startDate && v.PageType == PageTypes.MovieDetail)
                .OrderByDescending(v => v.VisitedAt)
                .ToListAsync();

            
            return _mapper.Map<List<VisitorReportDto>>(logs);
        }




    }
}