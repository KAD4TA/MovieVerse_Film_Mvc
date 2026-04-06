using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Infrastructure.Repositories
{
    
    public class ActorRepository : IActorRepository
    {
        private readonly IApplicationDbContext _context;

        public ActorRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Actor>> GetAllAsync()
        {
            return await _context.Actors
                .Include(a => a.MovieActors)           // ← MovieCount için gerekli!
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Actor?> GetByIdAsync(Guid id)
        {
            return await _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                        .ThenInclude(m => m.Translations)   // ← Title resolver için gerekli!
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.ActorId == id);
        }

        public async Task<Actor?> GetActorWithMoviesAsync(Guid actorId)
        {
            return await _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                        .ThenInclude(m => m.Translations)   // ← Detay sayfası için
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.ActorId == actorId);
        }

        public async Task AddAsync(Actor actor) => await _context.Actors.AddAsync(actor);
        public async Task DeleteAsync(Guid id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null) _context.Actors.Remove(actor);
        }
    }
}