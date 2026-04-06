



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Entities.EntityTranslations;
using MovieMvcProject.Domain.Identity;

namespace MovieMvcProject.Application.Interfaces
{
    public interface IApplicationDbContext : IDisposable
    {
        
        DbSet<AppUser> Users { get; }
        DbSet<AppRole> Roles { get; } 

        DbSet<Movie> Movies { get; }
        DbSet<Comment> Comments { get; }

        DbSet<Actor> Actors { get; }
        DbSet<Director> Directors { get; }

        DbSet<Wishlist> Wishlists { get; }
        DbSet<MovieActor> MovieActors { get; }
       
        DbSet<MovieTranslation> MovieTranslations { get; } 
        DbSet<LogEntry> Logs { get; }

        DbSet<MovieVisitLog> MovieVisitLogs { get; set; }

        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}