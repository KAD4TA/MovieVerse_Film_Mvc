using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Comments.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
    public class GetCommentsByMovieQueryHandler
        : IRequestHandler<GetCommentsByMovieQuery, PagedResult<CommentDtoResponse>>
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        private const string CacheKeyPrefix = "comments:movie";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);


        public GetCommentsByMovieQueryHandler(
            IElasticSearchService elasticSearchService,
            ICacheService cacheService,
            IMapper mapper,
            IApplicationDbContext context)
        {
            _elasticSearchService = elasticSearchService;
            _cacheService = cacheService;
            _mapper = mapper;
            _context = context;
        }


        public async Task<PagedResult<CommentDtoResponse>> Handle(GetCommentsByMovieQuery request, CancellationToken cancellationToken)
        {
            var commentsQuery = (IQueryable<Comment>)_context.Comments;

            // Veritabanından tüm ilişkileriyle çekiyoruz
            var allCommentsList = await commentsQuery
                .Include(c => c.User)
                .Include(c => c.Movie)
                    .ThenInclude(m => m.Translations)
                .Where(c => c.MovieId == request.MovieId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            var allDtos = _mapper.Map<List<CommentDtoResponse>>(allCommentsList);


            var filteredComments = allDtos.Where(c =>
                request.IsAdmin ||
                (string.Equals(c.UserId, request.UserId, StringComparison.OrdinalIgnoreCase)) || // Kullanıcı kendi yorumunu görmesi için
                c.Status == CommentStatus.Approved // Diğerleri sadece onaylı yorumları  görecek
            ).ToList();

            var pagedItems = filteredComments
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<CommentDtoResponse>(
                pagedItems,
                filteredComments.Count,
                request.PageNumber,
                request.PageSize);
        }
    }
}

