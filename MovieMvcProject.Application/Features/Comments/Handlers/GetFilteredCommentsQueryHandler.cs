using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Comments.Queries;
using MovieMvcProject.Application.Interfaces;

namespace MovieMvcProject.Application.Features.Comments.Handlers
{
   

    public class GetFilteredCommentsQueryHandler
    : IRequestHandler<
        GetFilteredCommentsQuery,
        PagedResult<CommentDtoResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetFilteredCommentsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<CommentDtoResponse>> Handle(
            GetFilteredCommentsQuery request,
            CancellationToken cancellationToken)
        {
            //  Parent yorumlar
            var query = _context.Comments
                .Where(c => c.ParentId == null)
                .AsQueryable();

            if (request.MovieId.HasValue)
                query = query.Where(c =>
                    c.MovieId == request.MovieId);

            if (!string.IsNullOrEmpty(request.UserId))
                query = query.Where(c =>
                    c.UserId == request.UserId);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
                query = query.Where(c =>
                    c.Content.Contains(request.Keyword));

            query = request.SortBy?.ToLower() switch
            {
                "date" => request.Descending
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),

                "rating" => request.Descending
                    ? query.OrderByDescending(c => c.MovieReview)
                    : query.OrderBy(c => c.MovieReview),

                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var totalCount =
                await query.CountAsync(cancellationToken);

            var parents =
                await query
                .Skip((request.PageNumber - 1) *
                       request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<CommentDtoResponse>(
                    _mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            if (!parents.Any())
                return new PagedResult<CommentDtoResponse>(
                    parents,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);

            //  Reply çek
            var parentIds =
                parents.Select(x => x.CommentId).ToList();

            var replies =
                await _context.Comments
                .Where(c =>
                    c.ParentId != null &&
                    parentIds.Contains(
                        c.ParentId.Value))
                .ProjectTo<CommentDtoResponse>(
                    _mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var flat =
                parents.Concat(replies).ToList();

            var tree =
                CommentTreeBuilder.BuildTree(flat);

            return new PagedResult<CommentDtoResponse>(
                tree,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
    }


}
