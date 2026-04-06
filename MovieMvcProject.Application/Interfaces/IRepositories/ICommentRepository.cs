


using MovieMvcProject.Application.Commons;
using MovieMvcProject.Domain.Entities; 
using MovieMvcProject.Domain.Enums;
using System.Linq.Expressions;

namespace MovieMvcProject.Application.Interfaces.IRepositories;

public interface ICommentRepository
{
    
    Task AddAsync(Comment entity); 
    
    Task UpdateAsync(Comment entity); 

    
    Task<bool> DeleteAsync(Guid commentId);

    Task<int> CountAsync(Expression<Func<Comment, bool>> predicate);



    Task DeleteCommentsByMovieId(Guid movieId);

    
    Task<Comment?> GetByIdAsync(Guid commentId); 

    
    Task<PagedResult<Comment>> GetCommentsByMovieWithUserStatusAsync(Guid movieId, string userId, int pageNumber, int pageSize);
    Task<List<Comment>> GetCommentsByIdsAsync(List<Guid> commentIds);
    Task<PagedResult<Comment>> GetCommentsByMovieAsync(
        Guid movieId,
        int pageNumber,
        int pageSize);
    Task<PagedResult<Comment>> GetAllComments(int pageNumber, int pageSize, string? searchTerm, CommentStatus? status);

    Task<List<Comment>> FindAsync(
        Expression<Func<Comment, bool>> predicate);
}
