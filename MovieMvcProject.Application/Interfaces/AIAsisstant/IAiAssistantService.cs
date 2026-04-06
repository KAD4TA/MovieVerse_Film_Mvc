using MovieMvcProject.Application.DTOs.RequestDto;

namespace MovieMvcProject.Application.Interfaces.AIAsisstant
{
    public interface IAiAssistantService
    {
        Task<MovieQueryIntent> AnalyzeUserPromptAsync(string userPrompt);
    }
}
