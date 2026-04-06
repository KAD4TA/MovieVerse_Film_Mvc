using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Interfaces.IRepositories


{
    public interface IDirectorRepository
    {
        Task<IEnumerable<Director>> GetAllAsync();
        Task<Director?> GetByIdAsync(Guid id);
        Task<Director?> GetByIdWithMoviesAsync(Guid id);
        Task AddAsync(Director director);
        Task DeleteAsync(Guid id);
    }
}