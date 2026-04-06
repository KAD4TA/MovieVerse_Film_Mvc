

using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.RequestDto;

namespace MovieMvcProject.Application.Interfaces.IRepositories;

public interface IMovieRepository
{
    
    Task AddAsync(Movie entity);

    Task UpdateAsync(Movie entity);

    Task<Movie?> GetMovieWithDetailsAsync(Guid id);

    
    Task<bool> DeleteAsync(Guid movieId);

  
    Task UpdateMovieRating(Guid movieId, double averageRating);

   
    Task<Movie?> GetByIdAsync(Guid id); // MovieDetailDto yerine Movie döndürülür

    
    Task<PagedResult<Movie>> GetAllMovies(
        string languageCode,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);


    Task<List<Movie>> GetAiRecommendedMoviesAsync(MovieQueryIntent intent, string langCode);

     
    Task<PagedResult<Movie>> GetByCategoryAsync(
    string categoryName,
    string languageCode,
    int pageNumber,
    int pageSize,
    CancellationToken ct);
    Task<PagedResult<Movie>> SearchMoviesAsync(
        string query,
        string languageCode,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default);
}