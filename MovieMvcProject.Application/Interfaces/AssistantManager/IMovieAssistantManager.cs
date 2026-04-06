using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Interfaces.AssistantManager
{
    public interface IMovieAssistantManager
    {
        Task<List<MovieDtoResponse>> GetSmartRecommendationsAsync(string userPrompt, string langCode);
    }
}
