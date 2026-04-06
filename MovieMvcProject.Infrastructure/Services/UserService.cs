

using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.WishList.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Application.Interfaces.Notification;
using MovieMvcProject.Domain.Identity;
using MovieMvcProject.Infrastructure.Services;

namespace MovieMvcProject.Application.Services;

public sealed class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localization;
    private readonly IFileService _fileService;
    private readonly ILogger<UserService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    private const string DefaultProfileImage = FileService.DefaultProfileImage;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<AppRole> roleManager,
        IMapper mapper,
        ILocalizationService localization,
        IFileService fileService,
        ILogger<UserService> logger,
        INotificationService notificationService,
        IMediator mediator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _localization = localization;
        _fileService = fileService;
        _logger = logger;
        _notificationService = notificationService;
        _mediator = mediator;
    }

    private string T(string key) => _localization.GetLocalizedString(key).Value ?? key;


    public async Task<bool> IsEmailTakenAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var existingUser = await _userManager.FindByEmailAsync(email);
        return existingUser != null;
    }

    #region LOGIN (IsActive = true + RememberMe destekli)
    public async Task<LoginResponseDto> Login(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return new LoginErrorResponseDto(T("InvalidCredentials"));
        }

        // 🔥 IsActive = true
        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        // Identity cookie ile giriş (Beni Hatırla destekli)
        await _signInManager.SignInAsync(user, isPersistent: dto.RememberMe);

        return new LoginSuccessResponseDto(user.FullName, user.Email)
        {
            IsSuccess = true,
            Message = T("LoginSuccess")
        };
    }
    #endregion

    #region LOGOUT (IsActive = false)
    public async Task<LogoutResponseDto> Logout(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = false;                    // 🔥 Logout'ta pasif yap
            await _userManager.UpdateAsync(user);
        }

        await _signInManager.SignOutAsync();

        return new LogoutResponseDto
        {
            IsSuccess = true,
            Message = T("LogoutSuccess")
        };
    }
    #endregion

    

    #region REGISTER (Otomatik giriş + IsActive = true + Kayıtlı email kontrolü)
    public async Task<UserResponseDto> Register(RegisterRequestDto dto)
    {
        string? uploadedImagePath = null;
        AppUser? createdUser = null;
        try
        {
            //  Kayıtlı email kontrolü (en başta!)
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Zaten kayıtlı email ile kayıt denemesi: {Email}", dto.Email);
                throw new Exception(T("UserAlreadyRegistered"));
            }

            var user = _mapper.Map<AppUser>(dto);
            user.UserName = dto.Email;
            user.EmailConfirmed = true;
            user.ProfileImageUrl = DefaultProfileImage;
            user.IsActive = true;

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            createdUser = user;
            await _userManager.AddToRoleAsync(user, "user");

            // Profil resmi
            if (dto.ProfileImageFile != null && dto.ProfileImageFile.Length > 0)
            {
                uploadedImagePath = await _fileService.SaveProfileImageAsync(dto.ProfileImageFile, user.Id);
                if (uploadedImagePath != DefaultProfileImage)
                {
                    user.ProfileImageUrl = uploadedImagePath;
                    await _userManager.UpdateAsync(user);
                }
            }

            // Otomatik giriş
            await _signInManager.SignInAsync(user, dto.RememberMe);

            // Admin bildirimi
            _ = _notificationService.NotifyAdminAsync(
                $"{user.FullName} ({user.Email}) {T("JoinedUs")}",
                T("NewUserRegistration"));

            var responseDto = _mapper.Map<UserResponseDto>(user);
            responseDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return responseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı kaydı başarısız: {Email}", dto.Email);
            if (!string.IsNullOrEmpty(uploadedImagePath) && uploadedImagePath != DefaultProfileImage)
                await _fileService.DeleteProfileImageAsync(uploadedImagePath);
            if (createdUser != null)
                await _userManager.DeleteAsync(createdUser);
            throw;
        }
    }
    #endregion

    #region DİĞER METOTLAR (tamamı eksiksiz)

    public async Task<UserListResponseDto> GetAllUsers(string searchTerm, int pageNumber, int pageSize)
    {
        try
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(search) ||
                                         u.Email.ToLower().Contains(search) ||
                                         u.Id.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserResponseDto>>(users);

            foreach (var dto in userDtos)
            {
                var user = users.First(u => u.Id == dto.Id);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }

            return new UserListResponseDto
            {
                IsSuccess = true,
                Users = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı listeleme hatası");
            return new UserListResponseDto { IsSuccess = false, Message = T("Error") };
        }
    }

    public async Task<RoleListResponseDto> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return new RoleListResponseDto
        {
            IsSuccess = true,
            Roles = _mapper.Map<List<RoleResponseDto>>(roles)
        };
    }

    public async Task<BaseResponse> UpdateUserRoles(UpdateUserRolesRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return new BaseResponse { IsSuccess = false, Message = T("UserNotFound") };

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRolesAsync(user, dto.NewRoles);

        return new BaseResponse
        {
            IsSuccess = result.Succeeded,
            Message = result.Succeeded ? T("RolesUpdated") : T("Error")
        };
    }

    public async Task<UserResponseDto?> GetByIdUser(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var dto = _mapper.Map<UserResponseDto>(user);
        dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
        return dto;
    }

    public async Task<ProfileUpdateResponseDto> UpdateProfile(ProfileUpdateRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return new ProfileUpdateResponseDto { IsSuccess = false, Message = T("UserNotFound") };

        try
        {
            // Şifre değiştirme
            if (!string.IsNullOrEmpty(dto.CurrentPassword) && !string.IsNullOrEmpty(dto.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    return new ProfileUpdateResponseDto
                    {
                        IsSuccess = false,
                        Message = string.Join(", ", passwordResult.Errors.Select(e => e.Description))
                    };
                }
            }

            // Profil resmi
            if (dto.ProfileImageFile != null && dto.ProfileImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != DefaultProfileImage)
                    await _fileService.DeleteProfileImageAsync(user.ProfileImageUrl);

                var newImagePath = await _fileService.SaveProfileImageAsync(dto.ProfileImageFile, user.Id);
                user.ProfileImageUrl = newImagePath;
            }

            // Diğer alanlar
            _mapper.Map(dto, user);

            var updateResult = await _userManager.UpdateAsync(user);

            return new ProfileUpdateResponseDto
            {
                IsSuccess = updateResult.Succeeded,
                Message = updateResult.Succeeded ? T("ProfileUpdateSuccess") : T("UpdateFailed")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profil güncelleme hatası - UserId: {UserId}", dto.UserId);
            return new ProfileUpdateResponseDto { IsSuccess = false, Message = T("UpdateFailed") };
        }
    }

    public async Task<DeleteUserResponseDto> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return new DeleteUserResponseDto { IsSuccess = false, Message = T("UserNotFound") };

        if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != DefaultProfileImage)
            await _fileService.DeleteProfileImageAsync(user.ProfileImageUrl);

        var result = await _userManager.DeleteAsync(user);

        return new DeleteUserResponseDto
        {
            IsSuccess = result.Succeeded,
            Message = result.Succeeded ? T("UserDeleted") : T("DeleteFailed")
        };
    }

    public async Task<PublicUserProfileResponseDto?> GetPublicProfileAsync(string userId, int pageNumber = 1, int pageSize = 12)
    {
        var user = await GetByIdUser(userId);
        if (user == null) return null;

        var wishlistQuery = new GetWishlistQuery(userId, pageNumber, pageSize);
        var wishlistResult = await _mediator.Send(wishlistQuery);

        return new PublicUserProfileResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            ProfileImageUrl = user.ProfileImageUrl,
            InstagramUrl = user.InstagramUrl,
            TwitterUrl = user.TwitterUrl,
            Wishlists = wishlistResult
        };
    }

    #endregion
}