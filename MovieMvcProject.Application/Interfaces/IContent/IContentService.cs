using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Interfaces.IContent
{
    public interface IContentService
    {
        
        Task<BaseResponse> GetAllContentAsync();

        
        Task<BaseResponse> AddContentAsync(object contentDto); 

        Task<BaseResponse> DeleteContentAsync(string id);
    }
}
