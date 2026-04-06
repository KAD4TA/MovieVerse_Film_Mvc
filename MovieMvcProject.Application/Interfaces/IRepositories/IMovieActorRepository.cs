
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Interfaces.IRepositories
{
    public interface IMovieActorRepository
    {
        Task<IEnumerable<MovieActor>> GetByMovieIdAsync(Guid movieId);
        Task<IEnumerable<MovieActor>> GetByActorIdAsync(Guid actorId);
        Task AddAsync(MovieActor movieActor);
        Task DeleteAsync(Guid movieId, Guid actorId);
    }

}
