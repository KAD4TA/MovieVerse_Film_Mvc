


using AutoMapper;
using MovieMvcProject.Application.DTOs.Dashboard;
using MovieMvcProject.Application.DTOs.LiveSearch;
using MovieMvcProject.Application.DTOs.LogDto;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Directors.Commands;
using MovieMvcProject.Application.Mapping.Resolvers;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Entities.EntityTranslations;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Identity;

namespace MovieMvcProject.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            

            CreateMap<Movie, TrendingMovieDtoResponse>()
                .IncludeBase<Movie, MovieDtoResponse>();

            CreateMap<MovieVisitLog, VisitorReportDto>()
                .ForMember(dest => dest.MovieTitle,
                    opt => opt.MapFrom<TranslationTitleResolver<MovieVisitLog, VisitorReportDto>, Movie>(
                        src => src.Movie))   

                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId ?? "Ziyaretçi"))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.UserAgent))
                .ForMember(dest => dest.VisitedAt, opt => opt.MapFrom(src => src.VisitedAt));

            // Movie -> MovieDtoResponse eşlemesi
            CreateMap<Movie, MovieDtoResponse>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId)) 
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.PosterUrl) ? "/images/no-poster.jpg" : src.PosterUrl)) 
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.Title, opt =>
                opt.MapFrom<TranslationTitleResolver<Movie, MovieDtoResponse>, Movie>(src => src))
                .ForMember(dest => dest.Category, opt => opt.MapFrom<LocalizedCategoryResolver, string>(src => src.Category.ToString()))
                .ForMember(dest => dest.IsOnSlider, opt => opt.MapFrom(src => src.IsOnSlider))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

            
            CreateMap<Movie, MovieSearchDocument>();

            CreateMap<Movie, MovieDetailDto>()


                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Title, opt =>
                 opt.MapFrom<TranslationTitleResolver<Movie, MovieDetailDto>, Movie>(src => src))
                .ForMember(dest => dest.Description, opt => opt.MapFrom<TranslationDescriptionResolver>())
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.DurationInMinutes))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.PosterUrl) ? "/images/no-poster.jpg" : src.PosterUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.MovieAvgReviewRate, opt => opt.MapFrom(src => src.MovieAvgReviewRate))

                // Yönetmen
                .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director))

                // Oyuncular (sadece Actor bilgisi)
                .ForMember(dest => dest.Actors, opt => opt.MapFrom(src => src.MovieActors.Select(ma => ma.Actor)))

                // Yorumlar 
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))

                
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src =>
                    src.Translations.Select(t => new MovieTranslation
                    {

                        MovieId = t.MovieId,
                        LanguageCode = t.LanguageCode,
                        Title = t.Title,
                        Description = t.Description
                        
                    }).ToList()))
                .MaxDepth(3)
                .PreserveReferences(); // AutoMapper'da referansları koruması için


            CreateMap<Actor, ActorEditDto>()
                .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.Movies, opt => opt.MapFrom((src, dest, _, context) =>
                    context.Mapper.Map<List<ActorMovieDto>>(src.MovieActors ?? new List<MovieActor>())));


            CreateMap<MovieSearchDocument, MovieDtoResponse>()
                  .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.Title, opt => opt.MapFrom((src, dest, destMember, context) =>
                  {
                      var lang = context.Items["LanguageCode"] as string ?? "tr";
                      return lang == "tr" ? src.TitleTr : src.TitleEn;
                  })) 
                  .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterPath))
                  .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.ReleaseYear))
                  .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))                                                                 
                  .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));


           

            CreateMap<Wishlist, WishlistDtoResponse>()
                .ForMember(dest => dest.AddedAt,
                    opt => opt.MapFrom(src => src.AddedAt))
                .ForMember(dest => dest.Movie,
                    opt => opt.MapFrom(src => src.Movie));


            CreateMap<CreateDirectorDto, Director>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl ?? "/profile-images/default-profile.png"))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.DirectorId, opt => opt.Ignore()) 
                .ForMember(dest => dest.DirectedMovies, opt => opt.Ignore()); // İlişkiler handler'da halledilecek


            CreateMap<Director, DirectorLookupDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DirectorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.MovieCount, opt => opt.MapFrom(src => src.DirectedMovies.Count()));

            CreateMap<Director, DirectorDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DirectorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl ?? "/profile-images/default-profile.png")) // Default atama için
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.Movies, opt => opt.Ignore()) // Handler'da manuel dolduruluyor
                .MaxDepth(2);


            CreateMap<UpdateDirectorCommand, Director>();

            // --- FİLM OLUŞTURMA (CREATE/UPDATE) ---
            CreateMap<CreateMovieDto, Movie>()
                .ForMember(dest => dest.MovieId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => Enum.Parse<Category>(src.Category)))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.DurationInMinutes)) // ← EKLENDİ!
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.PosterUrl) ? "/images/no-poster.jpg" : src.PosterUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))

                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => new List<MovieTranslation>
                {
                    new() { LanguageCode = "tr", Title = src.TurkishTitle, Description = src.TurkishDescription },
                    new() { LanguageCode = "en", Title = src.EnglishTitle, Description = src.EnglishDescription }
                }));

           

            

            CreateMap<UpdateMovieDto, Movie>()
                 .ForMember(dest => dest.MovieId, opt => opt.Ignore())
                 .ForMember(dest => dest.Translations, opt => opt.Ignore())
                 .ForMember(dest => dest.MovieActors, opt => opt.Ignore())
                 .ForMember(dest => dest.Director, opt => opt.Ignore()) // Director objesini ignore et
                 .ForMember(dest => dest.DirectorId, opt => opt.Ignore()) // Id'yi biz manuel set edeceğiz
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => Enum.Parse<Category>(src.Category)));

            // ProfileUpdateRequestDto → AppUser (Admin ve kullanıcı profili güncelleme için)
            
            CreateMap<ProfileUpdateRequestDto, AppUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender ?? Gender.None)) // null ise None atanacak
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore());

            CreateMap<RegisterRequestDto, AppUser>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                 .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore()) // Manuel yapacağız
                 .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Identity tarafından set edilecek
                 .ForMember(dest => dest.Comments, opt => opt.Ignore());

            


            CreateMap<AppUser, UserResponseDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ProfileImageUrl)
                        ? "/profile-images/default-profile.png"
                        : (src.ProfileImageUrl.StartsWith("/") ? src.ProfileImageUrl : "/" + src.ProfileImageUrl)))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            
            CreateMap<List<AppUser>, UserListResponseDto>()
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            
            CreateMap<Actor, ActorDtoResponse>();

            CreateMap<Actor, ActorDetailDto>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ActorId))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl)) 
                 .ForMember(dest => dest.Movies, opt => opt.Ignore());

            

            CreateMap<CreateActorDto, Actor>()
            
                .ForMember(dest => dest.ActorId, opt => opt.Ignore())
                .ForMember(dest => dest.MovieActors, opt => opt.Ignore())
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.AvatarUrl)
                    ? "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS06TGTOlnRnEavBmnT7MIC1fre5hAhE3HfTQ&s"
                    : src.AvatarUrl));


            

            // CreateCommentDto -> Comment Entity eşlemesi
            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.MovieReview, opt => opt.MapFrom(src => src.MovieReview))
                .ForMember(dest => dest.CommentId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Movie, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => CommentStatus.Pending));




            
            CreateMap<Comment, CommentDtoResponse>()

                //  Username
                .ForMember(dest => dest.Username,
                    opt => opt.MapFrom(src =>
                        src.User != null
                            ? (src.User.FullName ?? src.User.Email)
                            : "Misafir Kullanıcı"))

                //  Profil Image
                .ForMember(dest => dest.UserProfileImageUrl,
                    opt => opt.MapFrom(src =>
                        src.User != null &&
                        !string.IsNullOrEmpty(src.User.ProfileImageUrl)
                            ? src.User.ProfileImageUrl
                            : "/profile-images/default-profile.png"))

                //  Content
                .ForMember(dest => dest.Content,
                    opt => opt.MapFrom(src => src.Content))

                //  Date
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt))

                //  ReviewRate
                .ForMember(dest => dest.MovieReview,
                    opt => opt.MapFrom(src => src.MovieReview))

                //  Status text
                .ForMember(dest => dest.StatusDisplay,
                    opt => opt.MapFrom(src => src.Status.ToString()))

                //  MOVIE TITLE 
                .ForMember(dest => dest.MovieTitle,
                    opt => opt.MapFrom(src =>

                        src.Movie != null &&
                        src.Movie.Translations != null &&
                        src.Movie.Translations.Any()

                            ? (
                                src.Movie.Translations
                                    .Where(t => t.LanguageCode == "tr")
                                    .Select(t => t.Title)
                                    .FirstOrDefault()

                                ?? src.Movie.Translations
                                    .Where(t => t.LanguageCode == "en")
                                    .Select(t => t.Title)
                                    .FirstOrDefault()

                                ?? src.Movie.Translations
                                    .Select(t => t.Title)
                                    .FirstOrDefault()

                                ?? "İsimsiz Film"
                            )

                            : "İsimsiz Film"
                    ))

                // 🔥 Nested replies tree builder dolduracak
                .ForMember(dest => dest.Replies,
                    opt => opt.Ignore());


            
            CreateMap<CommentSearchDocument, CommentDtoResponse>()
                 .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.CommentId))
                 .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                 .ForMember(dest => dest.UserProfileImageUrl, opt => opt.MapFrom(src => src.UserProfileImageUrl))
                 .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                 .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<CommentStatus>(src.Status)));


            CreateMap<Actor, ActorSearchDocument>()
                .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.ProfilePath, opt => opt.MapFrom(src => $"/Admin/AdminActor/Edit/{src.ActorId}")) // ← DÜZELTİLDİ (AdminActor değil)
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace));

            // Entity → Edit DTO (GetActorForEditQueryHandler için)
            CreateMap<Actor, ActorEditDto>()
                .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.Movies, opt => opt.MapFrom(src => src.MovieActors));

            //  ActorMovie → ActorMovieDto (Film listesi için)
            CreateMap<MovieActor, ActorMovieDto>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Title,
                opt => opt.MapFrom<TranslationTitleResolver<MovieActor, ActorMovieDto>, Movie>(src => src.Movie)) // dil resolver buradan çalışır
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Movie.Year))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.Movie.PosterUrl));



            // Search Document → Diğer DTO'lar 
            CreateMap<ActorSearchDocument, ActorDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate));

            CreateMap<ActorSearchDocument, ActorListDto>()
                .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate));

            CreateMap<Actor, ActorListDto>()
                .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.MovieCount, opt => opt.MapFrom(src => src.MovieActors.Count));




            CreateMap<ActorSearchDocument, ActorDtoResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.PhotoUrl));

            CreateMap<DirectorSearchDocument, DirectorLookupDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DirectorId)) 
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.MovieCount, opt => opt.Ignore());

            CreateMap<Director, DirectorSearchDocument>()
                .ForMember(dest => dest.DirectorId, opt => opt.MapFrom(src => src.DirectorId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.ProfilePath, opt => opt.MapFrom(src => $"/Admin/AdminDirector/Edit/{src.DirectorId}"))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace));

            CreateMap<DirectorSearchDocument, LiveSearchResultDto>()
                .ForMember("Id", opt => opt.MapFrom(src => src.DirectorId.ToString()))
                .ForMember("Title", opt => opt.MapFrom(src => src.Name))
                .ForMember("Type", opt => opt.MapFrom(src => "Yönetmen"))
                .ForMember("Url", opt => opt.MapFrom(src => $"/Admin/AdminDirector/Edit/{src.DirectorId}"))
                .ForMember("PhotoUrl", opt => opt.MapFrom(src => src.PhotoUrl));


            CreateMap<ActorSearchDocument, LiveSearchResultDto>();

            CreateMap<LogEntry, LogDto>().ReverseMap();
        }
    }
}
