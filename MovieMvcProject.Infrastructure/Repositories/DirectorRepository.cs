using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.Interfaces; 
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Infrastructure.Repositories



{
    public class DirectorRepository : IDirectorRepository
    {
        private readonly IApplicationDbContext _context;

        public DirectorRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Director>> GetAllAsync()
        {
            return await _context.Directors
                .Include(d => d.DirectedMovies)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Director?> GetByIdAsync(Guid id)
        {
            return await _context.Directors
                .Include(d => d.DirectedMovies)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.DirectorId == id);
        }

        public async Task<Director?> GetByIdWithMoviesAsync(Guid id)
        {
            return await _context.Directors
                .Include(d => d.DirectedMovies)
                    .ThenInclude(m => m.Translations)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.DirectorId == id);
        }

        public async Task AddAsync(Director director) => await _context.Directors.AddAsync(director);

        public async Task DeleteAsync(Guid id)
        {
            var director = await _context.Directors.FindAsync(id);
            if (director != null)
                _context.Directors.Remove(director);
        }
    }
}