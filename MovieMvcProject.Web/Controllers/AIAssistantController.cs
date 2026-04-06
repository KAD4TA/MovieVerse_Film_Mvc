using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Interfaces.AssistantManager;

namespace MovieMvcProject.Web.Controllers
{
    [Route("AIAssistant")]
    public class AIAssistantController : Controller
    {
        private readonly IMovieAssistantManager _assistantManager;
        public AIAssistantController(IMovieAssistantManager assistantManager) => _assistantManager = assistantManager;



        [HttpGet("Suggest")]
        public IActionResult Suggest()
        {
            return View();
        }

        

        [HttpPost("GetSuggestions")]
        public async Task<IActionResult> GetSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Sorgu boş olamaz." });

            try
            {
                // İstek yapılan dili alıyoruz (örneğin "tr")
                var langCode = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                var recommendations = await _assistantManager.GetSmartRecommendationsAsync(query, langCode);

                // Film bulunamadıysa 404 dönüyoruz
                if (recommendations == null || !recommendations.Any())
                {
                    return StatusCode(404, new { message = "Kriterlerinize uygun film bulunamadı." });
                }

                return Json(recommendations);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // AI Limit aşımı
                return StatusCode(429, new { message = "Yapay zeka şu an çok yoğun, lütfen kısa bir süre bekleyin." });
            }
            catch (Exception ex)
            {
                // Diğer beklenmedik hatalar
                return StatusCode(500, new { message = "Bir hata oluştu." });
            }
        }

       
    }
}
