


using Elastic.Clients.Elasticsearch;
using FluentValidation;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.AIAsisstant;
using MovieMvcProject.Application.Interfaces.AssistantManager;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Dashboard;
using MovieMvcProject.Application.Interfaces.IContent;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Application.Interfaces.Logging;
using MovieMvcProject.Application.Interfaces.Notification;
using MovieMvcProject.Application.Interfaces.VisitorTracking;
using MovieMvcProject.Application.Mapping;
using MovieMvcProject.Application.Mapping.Resolvers;
using MovieMvcProject.Application.Services;
using MovieMvcProject.Domain.Identity;
using MovieMvcProject.Infrastructure.Persistence;
using MovieMvcProject.Infrastructure.Repositories;
using MovieMvcProject.Infrastructure.Services;
using MovieMvcProject.Infrastructure.Services.AIAssistant;
using MovieMvcProject.Infrastructure.Services.AssistantManager;
using MovieMvcProject.Infrastructure.Services.Caching;
using MovieMvcProject.Infrastructure.Services.Content;
using MovieMvcProject.Infrastructure.Services.Dashboard;
using MovieMvcProject.Infrastructure.Services.Indexing;
using MovieMvcProject.Infrastructure.Services.Localization;
using MovieMvcProject.Infrastructure.Services.Logging;
using MovieMvcProject.Infrastructure.Services.Notification;
using MovieMvcProject.Infrastructure.Services.VisitorTracking;
using MovieMvcProject.Web.Filters;
using MovieMvcProject.Web.Hubs;
using MovieMvcProject.Web.Middleware;
using MovieMvcProject.Web.ViewModelValidators;
using MovieMvcProject.Web.WebMapping;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Threading.RateLimiting;

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"[Serilog ERROR] {msg}"));
try
{
    var builder = WebApplication.CreateBuilder(args);
    // ====================== CONFIGURATION ======================
    var configuration = builder.Configuration;
    var defaultConn = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection eksik!");
    var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
    var esConn = configuration.GetConnectionString("Elasticsearch") ?? "http://localhost:9200";
    // ====================== SERILOG ======================
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());
    // ====================== SERVICES ======================
    builder.Services.AddHttpContextAccessor();
    // DbContext & Identity
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(defaultConn));
    builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    builder.Services.AddSignalR();
    // ====================== REDIS + DATA PROTECTION ======================
    var redisConfig = ConfigurationOptions.Parse(redisConn);
    redisConfig.AbortOnConnectFail = false;
    redisConfig.ConnectTimeout = 15000;
    redisConfig.SyncTimeout = 15000;
    redisConfig.AsyncTimeout = 15000;
    redisConfig.ReconnectRetryPolicy = new ExponentialRetry(500, 10000);
    var redisMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
    Console.WriteLine($"Redis bağlantı durumu: {(redisMultiplexer.IsConnected ? "BAĞLANDI" : "BAĞLANAMADI")}");
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = configuration.GetValue<string>("RedisCache:InstanceName") ?? "MovieMvc_";
    });
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redisMultiplexer, "MovieVerse-DataProtection-Keys")
        .SetApplicationName("MovieVerse")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
    // ====================== ELASTICSEARCH ======================
    builder.Services.AddSingleton<ElasticsearchClient>(sp =>
        new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(esConn))
            .RequestTimeout(TimeSpan.FromSeconds(30))));
    // ====================== AUTOMAPPER & MEDIATR ======================
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
        cfg.AddProfile<WebMappingProfile>();
    });
    builder.Services.AddTransient(typeof(TranslationTitleResolver<,>));
    builder.Services.AddTransient<TranslationDescriptionResolver>();
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly));
    // ====================== MVC + VALIDATION + LOCALIZATION ======================
    
    builder.Services.AddControllersWithViews(options =>
    {
        
        options.Filters.Add<FluentValidationFilter>();
    })
.AddViewLocalization()
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
    {
       
        return factory.Create(typeof(MovieMvcProject.Domain.Resources.ValidationResource));
    };
});


    builder.Services.AddValidatorsFromAssemblyContaining<RegisterViewModelValidator>();
    // ====================== SESSION & FILE UPLOAD ======================
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
    builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 5 * 1024 * 1024);
    // ====================== COOKIE AYARLARI ======================
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Error/Error403";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "MovieVerse.Auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";
    });


    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;   
        options.Password.RequiredLength = 8;             
        options.Password.RequiredUniqueChars = 1;
    });
    // ====================== LOCALIZATION ======================
    
   

    builder.Services.AddLocalization(options => options.ResourcesPath = "");
    var supportedCultures = new[] { "tr-TR", "en-US" };
    var localizationOptions = new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("tr-TR"),
        SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
        SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
    };

    
    localizationOptions.RequestCultureProviders.Clear();

    // Sonra sadece QueryString ve Cookie üzerinden dil değişimine izin veriyoruz
    localizationOptions.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
    localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());
    // ====================== RATE LIMITING  ======================
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // ★ Global Limiter → Tüm uygulamayı kapsayan gevşek kural (IP bazlı, 300 istek/dakika)
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 300,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        //Özel Politika → Register(spam kayıt koruması )
        options.AddPolicy("register", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 3,                    // 1 dakikada sadece 3 kayıt denemesi
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

        // ★ Özel Politika → Login için (brute-force koruması, IP bazlı)
        options.AddPolicy("login", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // ★ Özel Politika → Ağır veritabanı işlemleri (arama, liste, CRUD) için TokenBucket (IP bazlı)
        options.AddPolicy("heavy-db", httpContext =>
            RateLimitPartition.GetTokenBucketLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 50,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    });


    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;                    
        options.Providers.Add<BrotliCompressionProvider>(); 
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json", "text/css", "application/javascript", "text/html" });
    });

    builder.Services.AddResponseCaching();

    // ====================== DEPENDENCY INJECTION ======================
    builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    // Repositories
    builder.Services.AddScoped<ICommentRepository, CommentRepository>();
    builder.Services.AddScoped<IMovieRepository, MovieRepository>();
    builder.Services.AddScoped<IActorRepository, ActorRepository>();
    builder.Services.AddScoped<IMovieActorRepository, MovieActorRepository>();
    builder.Services.AddScoped<IDirectorRepository, DirectorRepository>();
    builder.Services.AddScoped<ILogQueryRepository, LogQueryRepository>();
    builder.Services.AddHttpClient<IAiAssistantService, GeminiAssistantService>();
    builder.Services.AddScoped<IMovieAssistantManager, MovieAssistantManager>();
    // Services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IMovieRatingService, MovieRatingService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IContentService, ContentService>();
    builder.Services.AddScoped<IVisitorTrackingService, VisitorTrackingService>();
    // Infrastructure
    builder.Services.AddScoped<IElasticSearchService, ElasticSearchService>();
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
    builder.Services.AddScoped<ILocalizationService, LocalizationService>();
    builder.Services.AddScoped<ILogService, LogService>();
    builder.Services.AddTransient<ICommentIndexingService, CommentIndexingService>();
    // ====================== APP BUILD ======================
    var app = builder.Build();
    // ====================== MIDDLEWARE ======================
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }
    app.UseStatusCodePagesWithReExecute("/Error/Error{0}");
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseResponseCompression();
    app.UseResponseCaching();
    app.UseRequestLocalization(localizationOptions);
    app.UseSerilogRequestLogging();
    app.UseSession();
    app.UseRateLimiter();                    // ← Rate limiting middleware 
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.Use(async (context, next) =>
    {
        if (context.Request.ContentType?.Contains("multipart/form-data") == true)
            context.Request.EnableBuffering();
        await next();
    });
    app.MapHub<AdminHub>("/adminHub");
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    // ====================== SEED & INDEXING ======================
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Seed ve Elasticsearch indexing başlatılıyor...");
            var context = services.GetRequiredService<ApplicationDbContext>();
            var elasticService = services.GetRequiredService<IElasticSearchService>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var config = services.GetRequiredService<IConfiguration>();
            // Roller
            foreach (var role in new[] { "admin", "user", "moderator" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole { Name = role });
                    logger.LogInformation("Rol oluşturuldu: {Role}", role);
                }
            }
            // Admin kullanıcı
            var adminEmail = config["AdminUser:Email"];
            var adminPassword = config["AdminUser:Password"];
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Sistem Yöneticisi",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    ProfileImageUrl = FileService.DefaultProfileImage
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "admin", "user", "moderator" });
                    logger.LogInformation("Admin kullanıcı oluşturuldu: {Email}", adminEmail);
                }
            }
            // Elasticsearch indexing
            await SeedElasticsearchAsync(elasticService, context, logger);
        }
        catch (HostAbortedException)
        {
            
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Uygulama başlatılamadı.");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılamadı.");
    throw;
}
// ====================== SEED METODU ======================
static async Task SeedElasticsearchAsync(IElasticSearchService elasticService, ApplicationDbContext context, ILogger<Program> logger)
{
    try
    {
        await elasticService.EnsureIndexAsync<DirectorSearchDocument>("directors");
        var directors = await context.Directors.AsNoTracking().ToListAsync();
        foreach (var d in directors)
        {
            var doc = new DirectorSearchDocument(
                DirectorId: d.DirectorId,
                Name: d.Name,
                PhotoUrl: d.PhotoUrl ?? "profile-images/default-profile.png",
                ProfilePath: $"/Admin/AdminDirector/Edit/{d.DirectorId}",
                Height: d.Height,
                BirthDate: d.BirthDate,
                BirthPlace: d.BirthPlace
            );
            await elasticService.IndexAsync(doc, "directors", d.DirectorId);
        }
        await elasticService.EnsureIndexAsync<ActorSearchDocument>("actors");
        var actors = await context.Actors.AsNoTracking().ToListAsync();
        foreach (var a in actors)
        {
            var doc = new ActorSearchDocument(
                id: a.ActorId,
                name: a.Name,
                photoUrl: a.AvatarUrl ?? "profile-images/default-profile.png",
                profilePath: $"/Admin/AdminActor/Edit/{a.ActorId}",
                height: a.Height,
                birthDate: a.BirthDate,
                birthPlace: a.BirthPlace
            );
            await elasticService.IndexAsync(doc, "actors", a.ActorId);
        }
        await elasticService.EnsureIndexAsync<MovieSearchDocument>("movies");
        var movies = await context.Movies.Include(m => m.Translations).AsNoTracking().ToListAsync();
        foreach (var m in movies)
        {
            var tr = m.Translations?.FirstOrDefault(t => t.LanguageCode == "tr");
            var en = m.Translations?.FirstOrDefault(t => t.LanguageCode == "en");
            var doc = new MovieSearchDocument(
                Id: m.MovieId,
                TitleTr: tr?.Title ?? "Başlıksız",
                TitleEn: en?.Title ?? "Untitled",
                DescriptionTr: tr?.Description ?? "",
                DescriptionEn: en?.Description ?? "",
                Rating: m.Rating,
                Category: m.Category.ToString(),
                PosterPath: m.PosterUrl ?? "images/no-poster.jpg",
                ReleaseYear: m.Year
            );
            await elasticService.IndexAsync(doc, "movies", m.MovieId);
        }
        await elasticService.EnsureIndexAsync<CommentSearchDocument>("comments");
        var comments = await context.Comments
            .Include(c => c.User)
            .AsNoTracking()
            .ToListAsync();
        foreach (var c in comments)
        {
            var commentDoc = new CommentSearchDocument
            {
                CommentId = c.CommentId,
                Content = c.Content,
                Username = c.User?.FullName ?? "Misafir Kullanıcı",
                UserProfileImageUrl = string.IsNullOrEmpty(c.User?.ProfileImageUrl)
                                        ? "/profile-images/default-profile.png"
                                        : c.User.ProfileImageUrl,
                MovieId = c.MovieId,
                UserId = c.UserId,
                CreatedAt = c.CreatedAt,
                Status = c.Status.ToString()
            };
            await elasticService.IndexAsync(commentDoc, "comments", c.CommentId);
        }
        logger.LogInformation("Elasticsearch indexing tamamlandı.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Elasticsearch indexing hatası.");
    }
}