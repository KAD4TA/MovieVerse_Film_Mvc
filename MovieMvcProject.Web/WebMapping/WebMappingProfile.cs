




using AutoMapper;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Actors.Commands;
using MovieMvcProject.Application.Features.Directors.Commands;
using MovieMvcProject.Application.Mapping.Resolvers;
using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Identity;
using MovieMvcProject.Web.Areas.Admin.Models;
using MovieMvcProject.Web.Models;

namespace MovieMvcProject.Web.WebMapping
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<MovieUpdateViewDto, MovieCreateUpdateViewModel>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationInMinutes))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => Enum.Parse<Category>(src.Category)))

                // Yönetmen Bilgileri
                .ForMember(dest => dest.ExistingDirectorId, opt => opt.MapFrom(src => src.Director.Id))
                .ForMember(dest => dest.ExistingDirectorName, opt => opt.MapFrom(src => src.Director.Name))
                .ForMember(dest => dest.DirectorPhotoUrl, opt => opt.MapFrom(src => src.Director.PhotoUrl))


                // OYUNCULAR (İsimlerin uyuştuğundan emin oluyoruz):
                .ForMember(dest => dest.Actors, opt => opt.MapFrom(src => src.Actors.Select(a => new ActorViewModel
                {
                    ActorId = a.ActorId,
                    Name = a.Name,
                    AvatarUrl = a.AvatarUrl
                }).ToList()));

            CreateMap<MovieDetailDto, MovieDetailViewModel>()
                .ForMember(dest => dest.MovieDetail, opt => opt.MapFrom(src => src))

                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.Actors, opt => opt.MapFrom(src => src.Actors));


            CreateMap<CreateActorViewModel, CreateActorDto>()
                .ReverseMap();

            // Director
            CreateMap<CreateDirectorViewModel, CreateDirectorDto>()
                .ReverseMap();

            // ViewModel'den DTO'ya dönüşüm
            CreateMap<MovieDetailViewModel.CommentItem, CreateCommentDto>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.MovieReview, opt => opt.MapFrom(src => (int)src.DisplayRating));




            
            CreateMap<Movie, MovieUpdateViewDto>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.DurationInMinutes))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))

                // Dil Çevirileri
                .ForMember(dest => dest.TurkishTitle, opt => opt.MapFrom(src =>
                 src.Translations.FirstOrDefault(t => t.LanguageCode == "tr") != null
                 ? src.Translations.First(t => t.LanguageCode == "tr").Title
                 : string.Empty))
                .ForMember(dest => dest.EnglishTitle, opt => opt.MapFrom(src =>
                 src.Translations.FirstOrDefault(t => t.LanguageCode == "en") != null
                 ? src.Translations.First(t => t.LanguageCode == "en").Title
                 : string.Empty))
                .ForMember(dest => dest.TurkishDescription, opt => opt.MapFrom(src =>
                 src.Translations.FirstOrDefault(t => t.LanguageCode == "tr") != null
                 ? src.Translations.First(t => t.LanguageCode == "tr").Description
                 : string.Empty))
                .ForMember(dest => dest.EnglishDescription, opt => opt.MapFrom(src =>
                 src.Translations.FirstOrDefault(t => t.LanguageCode == "en") != null
                 ? src.Translations.First(t => t.LanguageCode == "en").Description
                 : string.Empty))

                 // Yönetmen
                 .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director))

                 // OYUNCULAR 
                 .ForMember(dest => dest.Actors, opt => opt.MapFrom(src =>
                     src.MovieActors.Select(ma => new ActorDtoResponse
                     {
                         ActorId = ma.Actor.ActorId,
                         Name = ma.Actor.Name,
                         AvatarUrl = ma.Actor.AvatarUrl 
                     }).ToList()));


            CreateMap<ActorListDto, ActorListViewModel>();
            CreateMap<ActorEditDto, ActorEditViewModel>();
            CreateMap<ActorMovieDto, ActorMovieItemViewModel>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
                // int olan Year'ı string'e çeviriyoruz
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year.ToString()));

            // Yazma (Command) eşlemeleri: ViewModel -> Command
            CreateMap<UpdateActorCommand, Actor>();


            
            CreateMap<UpdateActorDto, Actor>();

            CreateMap<ActorEditViewModel, UpdateActorDto>();

            CreateMap<MovieDetailDto, MovieDetailViewModel.MovieDetailData>()
                 .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                 .ForMember(dest => dest.DirectorId, opt => opt.MapFrom(src => src.Director != null ? src.Director.Id : Guid.Empty))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))

                 .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                 .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))

                 .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
                 .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                 .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.DurationInMinutes))
                 .ForMember(dest => dest.MovieAvgReviewRate, opt => opt.MapFrom(src => src.MovieAvgReviewRate))
                 .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director != null ? src.Director.Name : string.Empty))
                 .ForMember(dest => dest.DirectorPhotoUrl, opt => opt.MapFrom(src => src.Director != null ? src.Director.PhotoUrl : string.Empty));



            CreateMap<ActorDtoResponse, ActorViewModel>();

            CreateMap<WishlistDtoResponse, WishlistViewModel>();
            
            CreateMap<PagedResult<WishlistDtoResponse>, PagedResult<WishlistViewModel>>()
                .ConvertUsing((src, dest, context) =>
                {
                    var items = context.Mapper.Map<List<WishlistViewModel>>(src.Items);
                    return new PagedResult<WishlistViewModel>(
                        items.AsReadOnly(),
                        src.TotalCount,
                        src.PageNumber,
                        src.PageSize);
                });

            CreateMap<DirectorDetailDto, DirectorDetailViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.Movies, opt => opt.MapFrom(src => src.Movies));
            
            CreateMap<CommentDtoResponse, MovieDetailViewModel.CommentItem>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CommentDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)) 
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username)) 
                .ForMember(dest => dest.UserProfilePictureUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.UserProfileImageUrl)
                    ? "/profile-images/default-profile.png"
                    : src.UserProfileImageUrl))
                .ReverseMap();

            // Admin paneli kullanıcı düzenleme: ViewModel → RequestDto
            // Admin paneli: ViewModel → RequestDto
            CreateMap<Areas.Admin.Models.UserUpdateViewModel, ProfileUpdateRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageFile, opt => opt.MapFrom(src => src.ProfileImage));

            

            CreateMap<ActorDtoResponse, MovieDetailViewModel.ActorItem>()
                 .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId))
                 .ForMember(dest => dest.ActorName, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                 .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Oyuncu"));

            

            // ViewModel'den DTO'ya dönüşüm
            CreateMap<MovieDetailViewModel.CommentItem, CreateCommentDto>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.MovieReview, opt => opt.MapFrom(src => (int)src.DisplayRating));

            // ===================================================
            // 👤 AUTH & KULLANICI İŞLEMLERİ
            // ===================================================
            CreateMap<RegisterViewModel, RegisterRequestDto>();
            CreateMap<LoginViewModel, LoginRequestDto>();

            
            // Profil Güncelleme (Kullanıcı tarafı)
            CreateMap<UserResponseDto, ProfileUpdateViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.ProfileImageUrl))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => Enum.Parse<Gender>(src.Gender, true)))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentPassword, opt => opt.Ignore())
                .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmNewPassword, opt => opt.Ignore());



            // SettingsPageViewModel → ProfileUpdateRequestDto
            CreateMap<SettingsPageViewModel, ProfileUpdateRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Controller'da set edilecek
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageFile, opt => opt.MapFrom(src => src.ProfileImageFile))
                .ForMember(dest => dest.CurrentPassword, opt => opt.MapFrom(src => src.CurrentPassword))
                .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.NewPassword));

            

            
            CreateMap<UserResponseDto, SettingsPageViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.ProfileImageUrl))
                
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                    src.Gender == null
                        ? Gender.None
                        : (Gender)Enum.Parse(typeof(Gender), src.Gender, true)))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentPassword, opt => opt.Ignore())
                .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmNewPassword, opt => opt.Ignore());

            CreateMap<UserResponseDto, Areas.Admin.Models.UserUpdateViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ProfileImageUrl)
                        ? "/profile-images/default-profile.png"
                        : src.ProfileImageUrl))

                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender ?? "None"));







            CreateMap<ProfileUpdateViewModel, ProfileUpdateRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageFile, opt => opt.MapFrom(src => src.ProfileImageFile))
                .ForMember(dest => dest.CurrentPassword, opt => opt.MapFrom(src => src.CurrentPassword))
                .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.NewPassword));

            // Admin paneli kullanıcı listesi
            CreateMap<UserResponseDto, Areas.Admin.Models.UserViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserPhoto, opt => opt.MapFrom(src => src.ProfileImageUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.RoleNames, opt => opt.MapFrom(src => src.Roles != null ? string.Join(", ", src.Roles) : ""));

            
            // AppUser'dan ViewModel'e dönüşüm 
            CreateMap<AppUser, MovieMvcProject.Web.Areas.Admin.Models.ManageRolesViewModel>()
                // ID eşleşmesi
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))

                // Resim eşleşmesi: AppUser'daki 'ProfileImageUrl' (veya UserPhoto) alanını alma
                
                .ForMember(dest => dest.UserPhoto, opt => opt.MapFrom(src => src.ProfileImageUrl))

                // İsim eşleşmesi: AppUser'daki 'FullName' alanını ViewModel'deki 'UserName'e atama
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName ?? src.UserName))

                // Diğer alanlar
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))


                
                .ForMember(dest => dest.AllRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            CreateMap<UserResponseDto, Areas.Admin.Models.ManageRolesViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserPhoto, opt => opt.MapFrom(src => src.ProfileImageUrl))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => src.Roles ?? new List<string>()));



            CreateMap<UserResponseDto, Areas.Admin.Models.UserUpdateViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                // Güvenli Enum Dönüşümü
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => Enum.Parse<Gender>(src.Gender, true)))
                // Profil Resmi Null Kontrolü
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ProfileImageUrl)
                        ? "/profile-images/default-profile.png"
                        : src.ProfileImageUrl));

            // ===================================================
            // 🎬 FİLM LİSTESİ VE KISA GÖSTERİM
            // ===================================================
            CreateMap<MovieDetailDto, MovieViewModel>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.Category, opt =>
                opt.MapFrom<LocalizedCategoryResolver, string>(src => src.Category.ToString()))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating));

            CreateMap<MovieDtoResponse, MovieViewModel>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.IsOnSlider, opt => opt.MapFrom(src => src.IsOnSlider))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating));

            // ===================================================
            // 🎭 AKTÖR HARİTALAMALARI
            // ===================================================
            CreateMap<ActorDtoResponse, ActorViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl));


            CreateMap<ActorViewModel, CreateActorDto>();

            CreateMap<PublicUserProfileResponseDto, UserPageViewModel>()
            .ForMember(dest => dest.Wishlists,
                       opt => opt.MapFrom(src => src.Wishlists));

            CreateMap<MovieCreateUpdateViewModel, CreateMovieDto>()
                .ForMember(dest => dest.TurkishTitle, opt => opt.MapFrom(src => src.TurkishTitle))
                .ForMember(dest => dest.TurkishDescription, opt => opt.MapFrom(src => src.TurkishDescription))
                .ForMember(dest => dest.EnglishTitle, opt => opt.MapFrom(src => src.EnglishTitle))
                .ForMember(dest => dest.EnglishDescription, opt => opt.MapFrom(src => src.EnglishDescription))
                .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .AfterMap((src, dest) =>
                {
                    // ====================== YÖNETMEN ======================
                    if (!string.IsNullOrWhiteSpace(src.DirectorName) &&
                        (!src.ExistingDirectorId.HasValue || src.ExistingDirectorId.Value == Guid.Empty))
                    {
                        dest.NewDirector = new CreateDirectorDto
                        {
                            Name = src.DirectorName,
                            PhotoUrl = src.DirectorPhotoUrl ?? "/profile-images/default-profile.png",
                            BirthDate = src.DirectorBirthDate,
                            BirthPlace = src.DirectorBirthPlace,
                            Height = src.DirectorHeightCm,

                        };
                    }

                    // ====================== OYUNCULAR ======================
                    if (src.Actors != null && src.Actors.Any())
                    {
                        // 1. Mevcut oyuncular → ExistingActorIds
                        dest.ExistingActorIds = src.Actors
                            .Where(a => a.ActorId.HasValue && a.ActorId.Value != Guid.Empty)
                            .Select(a => a.ActorId.Value)
                            .ToList();

                        //  Yeni oyuncular → Actors listesi (tüm alanlar dahil)
                        dest.Actors = src.Actors
                            .Where(a => !a.ActorId.HasValue || a.ActorId.Value == Guid.Empty)
                            .Select(a => new CreateActorDto
                            {
                                Name = a.Name,
                                AvatarUrl = a.AvatarUrl,
                                Height = a.Height,
                                BirthPlace = a.BirthPlace,
                                BirthDate = a.BirthDate
                            })
                            .ToList();
                    }
                });


            CreateMap<DirectorLookupDto, DirectorLookupViewModel>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate))
                .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.PhotoUrl))
                .ForMember(d => d.MovieCount, o => o.MapFrom(s => s.MovieCount));




            // Edit sayfası için
            CreateMap<DirectorDetailDto, DirectorEditViewModel>()
                .ForMember(d => d.DirectorId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.PhotoUrl))
                .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate))
                .ForMember(d => d.BirthPlace, o => o.MapFrom(s => s.BirthPlace))
                .ForMember(d => d.Height, o => o.MapFrom(s => s.Height))
                .ForMember(d => d.Movies, o => o.MapFrom(s => s.Movies.Items));// PagedResult içinden Items alıyoruz

            // Film listesi mapping (DirectorMovieItemViewModel)
            CreateMap<MovieDetailDto, DirectorMovieItemViewModel>()
                .ForMember(d => d.MovieId, o => o.MapFrom(s => s.MovieId))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Year, o => o.MapFrom(s => s.Year))
                .ForMember(d => d.PosterUrl, o => o.MapFrom(s => s.PosterUrl));


            CreateMap<DirectorEditViewModel, UpdateDirectorCommand>()
            .ForMember(dest => dest.DirectorId, opt => opt.MapFrom(src => src.DirectorId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.PhotoUrl))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.BirthPlace, opt => opt.MapFrom(src => src.BirthPlace))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height));



            // ====================== ACTOR MAPPINGS ======================


            CreateMap<MovieActor, ActorMovieDto>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.Title, opt =>
                    opt.MapFrom<TranslationTitleResolver<MovieActor, ActorMovieDto>, Movie>(src => src.Movie))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Movie != null ? src.Movie.Year : 0))
                .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.Movie.PosterUrl));

            // ====================== DIRECTOR MAPPINGS ======================
            CreateMap<MovieDtoResponse, DirectorMovieItemViewModel>()
                .ForMember(d => d.MovieId, o => o.MapFrom(s => s.MovieId))
                .ForMember(d => d.Year, o => o.MapFrom(s => s.Year))
                .ForMember(d => d.PosterUrl, o => o.MapFrom(s => s.PosterUrl));

            CreateMap<DirectorDetailDto, DirectorEditViewModel>()
                .ForMember(d => d.DirectorId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.PhotoUrl))
                .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate))
                .ForMember(d => d.BirthPlace, o => o.MapFrom(s => s.BirthPlace))
                .ForMember(d => d.Height, o => o.MapFrom(s => s.Height))
                .ForMember(d => d.Movies, opt => opt.MapFrom((src, dest, _, context) =>
                    context.Mapper.Map<List<DirectorMovieItemViewModel>>(
                        src.Movies?.Items ?? new List<MovieDtoResponse>()
                    )));

            CreateMap<MovieCreateUpdateViewModel, UpdateMovieDto>()
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.DurationMinutes))

                // Yönetmen Mantığı
                .ForMember(dest => dest.ExistingDirectorId, opt => opt.MapFrom(src => src.ExistingDirectorId))
                .ForMember(dest => dest.NewDirector, opt => opt.MapFrom(src =>
                    src.ExistingDirectorId.HasValue ? null : new CreateDirectorDto
                    {
                        Name = src.DirectorName,
                        PhotoUrl = src.DirectorPhotoUrl,
                        Height = src.DirectorHeightCm,
                        BirthDate = src.DirectorBirthDate,
                        BirthPlace = src.DirectorBirthPlace
                    }))
                // Mevcut Oyuncular (ID'si olanlar)
                .ForMember(dest => dest.ExistingActorIds, opt => opt.MapFrom(src =>
                src.Actors
                    .Where(a =>
                        a.ActorId.HasValue &&
                        a.ActorId.Value != Guid.Empty
                    )
                    .Select(a => a.ActorId.Value)
                    .ToList()))
                // Yeni Oyuncular (Sisteme ilk kez girecekler)
                .ForMember(dest => dest.Actors, opt => opt.MapFrom(src =>
                    src.Actors.Where(a => !a.ActorId.HasValue).Select(a => new CreateActorDto { Name = a.Name, AvatarUrl = a.AvatarUrl }).ToList()));

                        

            CreateMap<Areas.Admin.Models.UserUpdateViewModel, ProfileUpdateRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
                .ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
                .ForMember(dest => dest.ProfileImageFile, opt => opt.MapFrom(src => src.ProfileImage)); 

            CreateMap<SettingsPageViewModel, ProfileUpdateRequestDto>()
                .ForMember(dest => dest.ProfileImageFile, opt => opt.MapFrom(src => src.ProfileImageFile));
            
            CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));

            CreateMap<ActorEditViewModel, UpdateActorCommand>();
        }
    }
}



