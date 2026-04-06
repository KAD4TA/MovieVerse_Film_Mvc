
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<ValidationResource> _localizer;
    private readonly ElasticsearchClient _elasticClient;
    private const string MoviesIndexName = "movies";

    public MovieRepository(
        IApplicationDbContext context,
        IStringLocalizer<ValidationResource> localizer,
        ElasticsearchClient elasticClient)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    

    //  Yeni filmi veritabanı takibine alır.
    public async Task AddAsync(Movie entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await _context.Movies.AddAsync(entity);
    }

    public async Task<Movie?> GetMovieWithDetailsAsync(Guid id)
    {
        return await _context.Movies
            
            .Include(m => m.Translations)
            .Include(m => m.MovieActors)
            .ThenInclude(ma => ma.Actor)
            .Include(m => m.Director)
            .FirstOrDefaultAsync(m => m.MovieId == id);
    }

    // Var olan filmi günceller.
    public Task UpdateAsync(Movie entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        _context.Movies.Update(entity);
        return Task.CompletedTask;
    }

    //Belirtilen filmi siler. 
    public async Task<bool> DeleteAsync(Guid movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);

        if (movie == null)
            throw new NotFoundException(nameof(Movie), movieId, _localizer);

        _context.Movies.Remove(movie);
        return true;
    }

    // Film puanını günceller.
    public async Task UpdateMovieRating(Guid movieId, double averageRating)
    {
        var movie = await _context.Movies.FindAsync(movieId);

        if (movie == null)
            throw new NotFoundException(nameof(Movie), movieId, _localizer);

        movie.MovieAvgReviewRate = averageRating;

        _context.Movies.Update(movie);

        
        await _context.SaveChangesAsync();
    }

    

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        
        return await _context.Movies
            .Include(m => m.Translations)
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.Director)
            .Include(m => m.Comments)
                .ThenInclude(c => c.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.MovieId == id);
    }

    // Tüm filmleri getirir
    public async Task<PagedResult<Movie>> GetAllMovies(
        string languageCode,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        
        IQueryable<Movie> query = _context.Movies
            .AsNoTracking()
            .Include(m => m.Translations);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            
            query = query.Where(m => m.Translations.Any(t =>
                t.Title != null && t.Title.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var movies = await query
            .OrderByDescending(m => m.Year)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        
        return new PagedResult<Movie>(movies, totalCount, pageNumber, pageSize);
    }


    public async Task<List<Movie>> GetAiRecommendedMoviesAsync(MovieQueryIntent intent, string langCode)
    {
        // Gerekli tüm tabloları bağlıyoruz
        var query = _context.Movies
            .Include(m => m.Translations)
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.Director) // Yönetmen tablosu
            .AsQueryable();

        //  AI'dan gelen tüm metinsel ifadeleri bir havuzda toplayalım
        var searchTerms = new List<string>();
        if (!string.IsNullOrEmpty(intent.ActorName)) searchTerms.Add(intent.ActorName.ToLower().Trim());
        if (!string.IsNullOrEmpty(intent.DirectorName)) searchTerms.Add(intent.DirectorName.ToLower().Trim());
        if (!string.IsNullOrEmpty(intent.SemanticSearch)) searchTerms.Add(intent.SemanticSearch.ToLower().Trim());

        //  Arama Mantığı 
        if (searchTerms.Any())
        {
            query = query.Where(m =>
                // A: Çevirilerde (Başlık ve Açıklama) arama
                m.Translations.Any(t => searchTerms.Any(term =>
                    t.Title.ToLower().Contains(term) ||
                    t.Description.ToLower().Contains(term))) ||

                // B: Oyuncu isimlerinde arama (MovieActors -> Actor -> Name)
                m.MovieActors.Any(ma => searchTerms.Any(term =>
                    ma.Actor.Name.ToLower().Contains(term))) ||

                // C: Yönetmen isminde arama (Director -> Name)
                (m.Director != null && searchTerms.Any(term =>
                    m.Director.Name.ToLower().Contains(term)))
            );
        }

        
       
        if (!string.IsNullOrEmpty(intent.Category) && Enum.TryParse<Category>(intent.Category, true, out var catEnum))
        {
            query = query.Where(m => m.Category == catEnum);
        }

        // Yıl (AI bazen 0 gönderdiği için 1900 sınırı var)
        if (intent.MinYear.HasValue && intent.MinYear > 1900)
        {
            query = query.Where(m => m.Year >= intent.MinYear.Value);
        }

        // Puan
        if (intent.MinRating.HasValue && intent.MinRating > 0)
        {
            query = query.Where(m => m.Rating >= intent.MinRating.Value);
        }

        //  Sonuçları Getirme (Puanı en yüksek olandan başla)
        var results = await query
            .AsSplitQuery()
            .OrderByDescending(m => m.Rating)
            .Take(10)
            .ToListAsync();

        //  "Boş Dönme" Mantığı: Eğer hiçbir şey bulunamadıysa, 
        
        if (!results.Any())
        {
            return await _context.Movies
                .Include(m => m.Translations)
                .AsSplitQuery()
                .OrderByDescending(m => m.Rating)
                .Take(3)
                .ToListAsync();
        }

        return results;
    }



    public async Task<PagedResult<Movie>> GetByCategoryAsync(
    string categoryName,
    string languageCode,
    int pageNumber,
    int pageSize,
    CancellationToken ct)
    {
        //  String olarak gelen kategori ismini Enum'a çeviriyoruz (Örn: "Action" -> Category.Action)
        if (!Enum.TryParse<Category>(categoryName, true, out var categoryEnum))
        {
            return PagedResult<Movie>.Empty(pageNumber, pageSize);
        }

        var query = _context.Movies
            .AsNoTracking()
            .Where(m => m.Category == categoryEnum) 
            .Include(m => m.Translations);

        var totalCount = await query.CountAsync(ct);
        var movies = await query
            .OrderByDescending(m => m.Year)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Movie>(movies, totalCount, pageNumber, pageSize);
    }





    //Elasticsearch ile gelişmiş arama sonuçlarını Entity olarak getirir. 
    public async Task<PagedResult<Movie>> SearchMoviesAsync(
        string query,
        string languageCode,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return PagedResult<Movie>.Empty(pageNumber, pageSize);

        languageCode = languageCode?.ToLowerInvariant() == "tr" ? "tr" : "en";
        var from = (pageNumber - 1) * pageSize;

        // Elasticsearch index'indeki alan isimleri
        var mainTitle = languageCode == "tr" ? "titleTr" : "titleEn";
        var mainDesc = languageCode == "tr" ? "descriptionTr" : "descriptionEn";
        var fallbackTitle = languageCode == "tr" ? "titleEn" : "titleTr";

      

        var searchResponse = await _elasticClient.SearchAsync<Movie>(s => s
            .Indices(MoviesIndexName)
            .From(from)
            .Size(pageSize)
            .TrackTotalHits(true)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sq => sq.MultiMatch(mm => mm
                            .Fields(mainTitle + " " + mainDesc)
                            .Query(query)
                            .Type(TextQueryType.BoolPrefix)
                        ),
                        sq => sq.MultiMatch(mm => mm
                            .Fields(fallbackTitle)
                            .Query(query)
                            .Type(TextQueryType.BoolPrefix)
                        ),
                        sq => sq.Match(m => m
                            .Field(languageCode == "tr" ? "titleTr" : "titleEn")
                            .Query(query)
                            .Boost(10)
                        )
                    )
                    .MinimumShouldMatch(1)
                )
            ),
            ct);

        if (!searchResponse.IsValidResponse || searchResponse.Documents == null || !searchResponse.Documents.Any())
            return PagedResult<Movie>.Empty(pageNumber, pageSize);

        var totalCount = searchResponse.Total > int.MaxValue ? int.MaxValue : (int)searchResponse.Total;

        //  Elasticsearch sonuçları (Movie belgeleri) doğrudan Entity olarak döndürülür.
        var movies = searchResponse.Documents.ToList();

        return new PagedResult<Movie>(movies, totalCount, pageNumber, pageSize);

    }
}