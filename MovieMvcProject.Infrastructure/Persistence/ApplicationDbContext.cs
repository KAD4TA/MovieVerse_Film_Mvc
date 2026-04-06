using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Entities.EntityTranslations;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Identity;

namespace MovieMvcProject.Infrastructure.Persistence
{
    // IdentityDbContext<AppUser, AppRole, string> kullanarak Identity'yi yapılandırıyoruz.
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, string>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // =======================================================
        // IApplicationDbContext Arayüzü ve DbSet Tanımları
        // =======================================================

        // Identity DbSet'leri
        public new DbSet<AppUser> Users => base.Users;
        public new DbSet<AppRole> Roles => base.Roles;

        // Core Entity DbSet'leri
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<MovieTranslation> MovieTranslations { get; set; }

        public DbSet<MovieVisitLog> MovieVisitLogs { get; set; } = null!;

        // Serilog Log Kayıtları DbSet'i (MSSQL Logs tablosu için)
        public DbSet<LogEntry> Logs { get; set; }

        public override DatabaseFacade Database => base.Database;

        // =======================================================
        // Model Oluşturma ve Konfigürasyon
        // =======================================================
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity tablolarının doğru oluşturulması için ZORUNLU çağrı.
            base.OnModelCreating(builder);

            // --- İLİŞKİ YAPILANDIRMALARI ---
            builder.Entity<Movie>()
                .Property(m => m.PosterUrl)
                .IsRequired(false);
            // MovieActor Many-to-Many
            builder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });
            builder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);
            builder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);


            builder.Entity<Movie>(entity =>
            {
                entity.HasOne(m => m.Director)
                      .WithMany(d => d.DirectedMovies)
                      .HasForeignKey(m => m.DirectorId)
                      .IsRequired(false)           
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Comment -> Movie 
            builder.Entity<Comment>()
                .HasOne(c => c.Movie)
                .WithMany(m => m.Comments)
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → Replies (Self Reference)
            builder.Entity<Comment>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // MovieTranslation Many-to-One
            builder.Entity<MovieTranslation>()
                .HasOne(t => t.Movie)
                .WithMany(m => m.Translations)
                .HasForeignKey(t => t.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.Entity<MovieTranslation>()
                .Property(t => t.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            // --- VERİ TİPİ DÖNÜŞÜMLERİ ---

            // AppUser Gender enum conversion (string olarak kaydetmek için)
            builder.Entity<AppUser>()
                .Property(u => u.Gender)
                .HasConversion(
                    v => v.ToString(),
                    v => (Gender)System.Enum.Parse(typeof(Gender), v, true)
                );

            // AppUser BirthDate (DateTime UTC) conversion 
            
            builder.Entity<AppUser>(entity =>
            {
                entity.Property(e => e.BirthDate)
                      .HasConversion(
                          v => v.HasValue ? System.DateTime.SpecifyKind(v.Value, System.DateTimeKind.Utc) : (System.DateTime?)null,
                          v => v.HasValue ? System.DateTime.SpecifyKind(v.Value, System.DateTimeKind.Utc) : (System.DateTime?)null
                      );
            });

            
            builder.Entity<Wishlist>().ToTable("Wishlists");


            builder.Entity<LogEntry>(entity =>
            {
                entity.ToTable("Logs", "dbo");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id");

                entity.Property(e => e.Message)
                    .HasColumnName("Message")
                    .IsRequired();

                entity.Property(e => e.MessageTemplate)
                    .HasColumnName("MessageTemplate");

                entity.Property(e => e.Level)
                    .HasColumnName("Level")
                    .HasMaxLength(128);

                entity.Property(e => e.TimeStamp)
                    .HasColumnName("TimeStamp");

                entity.Property(e => e.Exception)
                    .HasColumnName("Exception");

                entity.Property(e => e.Properties)
                    .HasColumnName("Properties")
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.DeviceType)
                    .HasColumnName("DeviceType")
                    .HasMaxLength(50);

                entity.Property(e => e.Environment)
                    .HasColumnName("Environment")
                    .HasMaxLength(50);

                entity.Property(e => e.Application)
                    .HasColumnName("Application")
                    .HasMaxLength(50);
            });
            builder.Entity<LogEntry>()
                .HasIndex(l => l.TimeStamp)
                .IsDescending(true) 
                .HasDatabaseName("IX_Logs_TimeStamp_DESC");


            builder.Entity<MovieVisitLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(v => v.VisitedAt);
                entity.HasIndex(v => v.MovieId);
                entity.HasIndex(v => new { v.VisitedAt, v.PageType });
            });


            builder.Entity<Wishlist>()
        .HasKey(w => new { w.MovieId, w.UserId });

            builder.Entity<Wishlist>()
                .HasOne(w => w.Movie)
                .WithMany() 
                .HasForeignKey(w => w.MovieId);

            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId);

        }
    }
}