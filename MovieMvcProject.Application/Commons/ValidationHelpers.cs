
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace MovieMvcProject.Application.Common
{
    public static class ValidationHelpers
    {
        public static bool BeAValidUrl(string? url) =>
            !string.IsNullOrEmpty(url) &&
            Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        public static bool BeValidFile(IFormFile? file)
        {
            // Dosya zorunlu değilse, null olması geçerlidir.
            if (file == null || file.Length == 0) return true;

            // Dosya boyutu kontrolü: 5 MB olarak sınırlandı (FileService ile uyumlu)
            const int maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxFileSize) return false;

            // İzin verilen uzantılar (FileService'daki MIME tiplerine karşılık gelenler)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            return !string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension);
        }

        public static bool BeAValidUrlOrEmpty(string? url) =>
            string.IsNullOrEmpty(url) || BeAValidUrl(url);

        public static readonly Regex StrongPasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])[A-Za-z\d!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?~`]{8,}$",
            RegexOptions.Compiled);

        
        public static bool NotCommonPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false; 
            var commonPasswords = new[] { "123456", "password", "qwerty", "abc123", "admin", "letmein", "welcome", "12345678", "123456789" };
            return !commonPasswords.Any(p => password.Equals(p, StringComparison.OrdinalIgnoreCase));
        }
    }
}