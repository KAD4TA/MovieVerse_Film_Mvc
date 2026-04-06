using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Infrastructure.Repositories
{
    public class MovieActorRepository : IMovieActorRepository
    {
        private readonly IApplicationDbContext _context;

        public MovieActorRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MovieActor>> GetByMovieIdAsync(Guid movieId)
        {
            return await _context.MovieActors
                .Include(ma => ma.Actor)
                .Where(ma => ma.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovieActor>> GetByActorIdAsync(Guid actorId)
        {
            return await _context.MovieActors
                .Include(ma => ma.Movie)
                .Where(ma => ma.ActorId == actorId)
                .ToListAsync();
        }

        public async Task AddAsync(MovieActor movieActor)
        {
            await _context.MovieActors.AddAsync(movieActor);
        }

        public async Task DeleteAsync(Guid movieId, Guid actorId)
        {
            var entity = await _context.MovieActors
                .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (entity != null)
            {
                _context.MovieActors.Remove(entity);
            }
        }
    }

}
