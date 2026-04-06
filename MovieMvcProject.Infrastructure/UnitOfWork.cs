

using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.IRepositories;

namespace MovieMvcProject.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _context;

    public IMovieRepository Movies { get; }
    public IActorRepository Actors { get; }
    public ICommentRepository Comments { get; }
    public IMovieActorRepository MovieActors { get; }

    public IDirectorRepository Directors { get; }
    public UnitOfWork(
        IApplicationDbContext context,
        IMovieRepository movieRepository,
        IActorRepository actorRepository,
        ICommentRepository commentRepository,
        IMovieActorRepository movieActorRepository,
        IDirectorRepository directorRepsitory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Movies = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
        Actors = actorRepository ?? throw new ArgumentNullException(nameof(actorRepository));
        Comments = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        MovieActors = movieActorRepository ?? throw new ArgumentNullException(nameof(movieActorRepository));
        Directors = directorRepsitory ?? throw new ArgumentNullException(nameof(directorRepsitory));
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public void Dispose()
        => _context.Dispose();
}
