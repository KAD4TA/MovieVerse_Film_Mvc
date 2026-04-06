using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Interfaces.IContent;

namespace MovieMvcProject.Infrastructure.Services.Content
{
    public class ContentService : IContentService
    {
        private readonly ILogger<ContentService> _logger;

        
        public ContentService(ILogger<ContentService> logger)
        {
            _logger = logger;
        }

      

        public Task<BaseResponse> GetAllContentAsync()
        {
            _logger.LogInformation("Tüm içerikler çekiliyor.");

           
            return Task.FromResult(new BaseResponse
            {
                IsSuccess = true,
                Message = "İçerik listesi başarıyla çekildi (Test)."
            });
        }

        public Task<BaseResponse> AddContentAsync(object contentDto)
        {
            _logger.LogInformation("Yeni içerik ekleniyor.");

            return Task.FromResult(new BaseResponse
            {
                IsSuccess = true,
                Message = "İçerik ekleme işlemi tamamlandı (Test)."
            });
        }

        public Task<BaseResponse> DeleteContentAsync(string id)
        {
            _logger.LogInformation("İçerik siliniyor: {ContentId}", id);

            return Task.FromResult(new BaseResponse
            {
                IsSuccess = true,
                Message = $"İçerik ({id}) başarıyla silindi (Test)."
            });
        }
    }
}
