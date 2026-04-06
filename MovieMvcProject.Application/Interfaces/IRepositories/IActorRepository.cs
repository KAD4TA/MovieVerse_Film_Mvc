



using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Interfaces.IRepositories
{
    public interface IActorRepository
    {
        Task<IEnumerable<Actor>> GetAllAsync();
        Task<Actor?> GetByIdAsync(Guid id);

        Task<Actor?> GetActorWithMoviesAsync(Guid actorId);
        Task AddAsync(Actor actor);
        
        Task DeleteAsync(Guid id);
    }

}
