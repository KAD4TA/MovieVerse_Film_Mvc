
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Enums;
using System.Linq.Expressions;

namespace MovieMvcProject.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IApplicationDbContext _context;

    
    public CommentRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    // --- COMMAND (Yazma) Metotları ---

    public async Task AddAsync(Comment entity)
    {
        await _context.Comments.AddAsync(entity);
    }

    public Task UpdateAsync(Comment entity)
    {
        // Güncelleme tarihi takibi
        entity.UpdatedAt = DateTime.Now;

        _context.Comments.Update(entity);
        
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid commentId)
    {
        var entity = await _context.Comments.FindAsync(commentId);

        if (entity == null)
            return false;

        _context.Comments.Remove(entity);
        return true;
    }


    public async Task<int> CountAsync(Expression<Func<Comment, bool>> predicate)
    {
        return await _context.Comments.CountAsync(predicate);
    }
    public async Task DeleteCommentsByMovieId(Guid movieId)
    {
        var comments = await _context.Comments
            .Where(c => c.MovieId == movieId)
            .ToListAsync();

        if (comments.Any())
        {
            _context.Comments.RemoveRange(comments);
        }
        // SaveChanges çağrısı dışarıda beklenir (DeleteMovieCommandHandler'da).
    }

    // --- QUERY (Okuma) Metotları ---

    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        // Tek Entity döndürülür. DTO'ya haritalama Handler'da yapılır.
        return await _context.Comments
            .Include(c => c.User) // Gerekli navigasyon property'lerini dahil etme
            .Include(c => c.Movie)
            .ThenInclude(m => m.Translations)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
    }


    public async Task<List<Comment>> GetCommentsByIdsAsync(List<Guid> commentIds)
    {
        if (commentIds == null || !commentIds.Any())
            return new List<Comment>();

        var comments = await _context.Comments
            .Where(c => commentIds.Contains(c.CommentId))
            .Include(c => c.User)
            .Include(c => c.Movie)                
                .ThenInclude(m => m.Translations) 
            .AsSplitQuery()                       
            .ToListAsync();

        // Elasticsearch sıralamasını korumak için
        var orderedComments = commentIds
            .Join(comments, id => id, comment => comment.CommentId, (id, comment) => comment)
            .ToList();

        return orderedComments;
    }
    public async Task<PagedResult<Comment>> GetCommentsByMovieWithUserStatusAsync(Guid movieId, string userId, int pageNumber, int pageSize)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Where(c => c.MovieId == movieId && (c.Status == CommentStatus.Approved || c.UserId == userId))
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Comment>(items, totalCount, pageNumber, pageSize);
    }
    
    public async Task<PagedResult<Comment>> GetCommentsByMovieAsync(
    Guid movieId,
    int pageNumber,
    int pageSize)
    {
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.MovieId == movieId)
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.User)
            .Include(c => c.Movie)
                .ThenInclude(m => m.Translations); 

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Comment>(items, totalCount, pageNumber, pageSize);
    }









    public async Task<PagedResult<Comment>> GetAllComments(int pageNumber, int pageSize, string? searchTerm, CommentStatus? status)
    {
        int activePage = pageNumber < 1 ? 1 : pageNumber;
        int skipCount = (activePage - 1) * pageSize;

        
        var query = _context.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Movie)
                .ThenInclude(m => m.Translations)
            .AsSplitQuery() 
            .AsQueryable();

        
        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Content.Contains(searchTerm) ||
                                 c.User.UserName.Contains(searchTerm) ||
                                 c.Movie.Translations.Any(t => t.Title.Contains(searchTerm)));
        }

        
        query = query.OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip(skipCount).Take(pageSize).ToListAsync();

        return new PagedResult<Comment>(items, totalCount, activePage, pageSize);
    }


    public async Task<List<Comment>> FindAsync(
   Expression<Func<Comment, bool>> predicate)
    {
        return await _context.Comments
        .AsNoTracking() // Okuma işlemlerinde performansı artırır
        .Where(predicate)
        .Include(x => x.User)
        .Include(x => x.Movie)
            .ThenInclude(m => m.Translations)
        .ToListAsync();
    }

}



