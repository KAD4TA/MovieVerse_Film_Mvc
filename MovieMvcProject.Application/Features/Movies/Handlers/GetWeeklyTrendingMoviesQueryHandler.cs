using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class GetWeeklyTrendingMoviesQueryHandler : IRequestHandler<GetWeeklyTrendingMoviesQuery, List<TrendingMovieDtoResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetWeeklyTrendingMoviesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TrendingMovieDtoResponse>> Handle(GetWeeklyTrendingMoviesQuery request, CancellationToken cancellationToken)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-7);

            // 1. Ziyaret loglarından en çok izlenen MovieId'leri ve sayılarını bulalım
            var trendingLogData = await _context.MovieVisitLogs
                .AsNoTracking()
                .Where(v => v.VisitedAt >= startDate && v.PageType == PageTypes.MovieDetail)
                .GroupBy(v => v.MovieId)
                .OrderByDescending(g => g.Count())
                .Take(request.Limit)
                .Select(g => new { MovieId = g.Key, ViewCount = g.Count() })
                .ToListAsync(cancellationToken);

            if (!trendingLogData.Any()) return new List<TrendingMovieDtoResponse>();

            var trendingMovieIds = trendingLogData.Select(x => x.MovieId).ToList();

            // 2. Bu filmleri Çoklu Dil (Translation) desteğiyle veritabanından çekelim
            var movies = await _context.Movies
                .AsNoTracking()
                .Include(m => m.Translations) // Çevirileri dahil et
                .Where(m => trendingMovieIds.Contains(m.MovieId))
                .ToListAsync(cancellationToken);

            var result = new List<TrendingMovieDtoResponse>();

            // 3. AutoMapper ve manuel atamaları birleştirerek listeyi oluşturalım
            foreach (var logData in trendingLogData)
            {
                var movie = movies.FirstOrDefault(m => m.MovieId == logData.MovieId);
                if (movie != null)
                {
                    // Temel map işlemini AutoMapper halleder (Movie -> TrendingMovieDtoResponse)
                    var mappedDto = _mapper.Map<TrendingMovieDtoResponse>(movie);

                    // Dile göre başlık atamasını yapalım (Eğer AutoMapper'da zaten ayarladıysan bu satıra gerek yok)
                    mappedDto.Title = movie.Translations?.FirstOrDefault(t => t.LanguageCode == request.LangCode)?.Title ?? movie.TitleTr;

                    // Ziyaretçi sayısını ekleyelim
                    mappedDto.WeeklyViewCount = logData.ViewCount;

                    result.Add(mappedDto);
                }
            }

            return result;
        }
    }
}
