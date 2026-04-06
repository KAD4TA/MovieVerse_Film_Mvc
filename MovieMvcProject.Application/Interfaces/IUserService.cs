using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Interfaces
{
    public interface IUserService
    {


        Task<bool> IsEmailTakenAsync(string email);
        Task<UserResponseDto?> GetByIdUser(string? id);

        Task<UserResponseDto> Register(RegisterRequestDto dto);

        Task<LoginResponseDto> Login(LoginRequestDto dto);

        Task<LogoutResponseDto> Logout(string userId);

        Task<DeleteUserResponseDto> DeleteUser(string id);


        Task<UserListResponseDto> GetAllUsers(string searchTerm, int pageNumber, int pageSize);

        Task<PublicUserProfileResponseDto?> GetPublicProfileAsync(string userId, int pageNumber = 1, int pageSize = 12);

 
        Task<ProfileUpdateResponseDto> UpdateProfile(ProfileUpdateRequestDto user);

        Task<RoleListResponseDto> GetAllRoles();
        Task<BaseResponse> UpdateUserRoles(UpdateUserRolesRequestDto dto);
    }
}