

using MovieMvcProject.Application.Interfaces.IRepositories;

namespace MovieMvcProject.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IMovieRepository Movies { get; }
    IActorRepository Actors { get; }
    ICommentRepository Comments { get; }
    IMovieActorRepository MovieActors { get; }
    IDirectorRepository Directors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}