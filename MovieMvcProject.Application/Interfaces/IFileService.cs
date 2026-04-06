using Microsoft.AspNetCore.Http;


namespace MovieMvcProject.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveProfileImageAsync(IFormFile? file, string userId);
        Task DeleteProfileImageAsync(string? imagePath);
    }


}
