using AutoMapper;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Interfaces.AIAsisstant;
using MovieMvcProject.Application.Interfaces.AssistantManager;
using MovieMvcProject.Application.Interfaces.IRepositories;

namespace MovieMvcProject.Infrastructure.Services.AssistantManager
{
    
    public class MovieAssistantManager : IMovieAssistantManager
    {
        private readonly IAiAssistantService _aiService;
        private readonly IMovieRepository _movieRepo;
        private readonly IMapper _mapper; // MAPPER EKLENDİ

        public MovieAssistantManager(IAiAssistantService aiService, IMovieRepository movieRepo, IMapper mapper)
        {
            _aiService = aiService;
            _movieRepo = movieRepo;
            _mapper = mapper;
        }

        
        public async Task<List<MovieDtoResponse>> GetSmartRecommendationsAsync(string userPrompt, string langCode)
        {
            var intent = await _aiService.AnalyzeUserPromptAsync(userPrompt);
            var movies = await _movieRepo.GetAiRecommendedMoviesAsync(intent, langCode);

            
            return _mapper.Map<List<MovieDtoResponse>>(movies, opt => {
                opt.Items["LanguageCode"] = langCode;
            });
        }
    }
}
