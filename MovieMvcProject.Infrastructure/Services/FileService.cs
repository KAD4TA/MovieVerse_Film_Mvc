
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Interfaces;

namespace MovieMvcProject.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    public const string DefaultProfileImage = "/profile-images/default-profile.png"; 

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveProfileImageAsync(IFormFile? file, string? userId = null)
    {
        //  Kritik Kontrol: Dosya gelmiş mi?
        if (file == null || file.Length == 0) return "/profile-images/default-profile.png";

        try
        {
            //  Yol Kontrolü: Klasör var mı?
            var root = _environment.WebRootPath;
            if (string.IsNullOrEmpty(root)) throw new Exception("WebRootPath null!");

            var folderPath = Path.Combine(root, "profile-images");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            //  Dosya Adı: Guid kullanarak çakışmayı önleme
            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"user_{userId ?? "0"}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folderPath, fileName);

            //  Kayıt: 'using' bloğu önemli, dosya kilidini açar
            using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            return $"/profile-images/{fileName}";
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "DOSYA KAYDEDİLEMEDİ!");
            return "/profile-images/default-profile.png";
        }
    }

    public async Task DeleteProfileImageAsync(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || imagePath.Contains("default-profile.png"))
            return;

        try
        {
            var cleanPath = imagePath.TrimStart('/');
            var fullPath = Path.Combine(_environment.WebRootPath, cleanPath);

            if (File.Exists(fullPath))
            {
                
                File.Delete(fullPath);
                _logger.LogInformation("Dosya silindi: {Path}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya silme hatası");
        }
        await Task.CompletedTask;
    }
}