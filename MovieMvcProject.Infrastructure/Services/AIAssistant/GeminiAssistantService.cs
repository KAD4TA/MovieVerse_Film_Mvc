using Microsoft.Extensions.Configuration;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.Interfaces.AIAsisstant;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace MovieMvcProject.Infrastructure.Services.AIAssistant
{
    public class GeminiAssistantService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiAssistantService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApi:ApiKey"]
                      ?? throw new InvalidOperationException("Gemini API Key eksik!");
        }

        
        public async Task<MovieQueryIntent> AnalyzeUserPromptAsync(string userPrompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = GeneratePrompt(userPrompt) } } } },
                model = "models/gemini-2.5-flash-lite",
                generationConfig = new { response_mime_type = "application/json" }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            // 429 Hatasını kontrol etme ve fırlatma
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Rate limit exceeded", null, System.Net.HttpStatusCode.TooManyRequests);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Google API Hatası ({response.StatusCode}): {errorDetail}");
                
                return new MovieQueryIntent();
            }

            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(jsonResponse);

            if (result?.candidates == null || result.candidates.Count == 0) return new MovieQueryIntent();

            string cleanJson = result.candidates[0].content.parts[0].text;
            if (cleanJson.Contains("```"))
            {
                cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();
            }

            try
            {
                return JsonConvert.DeserializeObject<MovieQueryIntent>(cleanJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("JSON ayrıştırma hatası: " + ex.Message);
                return new MovieQueryIntent();
            }
        }


        private string GeneratePrompt(string userPrompt)
        {
            return $@"Kullanıcıdan gelen ham metin: '{userPrompt}'
    Sen bir SQL uzmanı gibi davran. Bu metni analiz et ve SADECE JSON dön. 
    Kurallar:
    - 'SemanticSearch': 'film', 'aksiyon' gibi genel kelimeleri ASLA yazma. Sadece konuyu yaz. Boşsa null bırak.
    - 'Category': Sadece senin Enum isimlerini yaz (Action, Drama, SciFi gibi).
    - 'ActorName' ve 'DirectorName': ASLA liste ([]) gönderme, sadece tek bir isim (string) gönder.

    JSON Örneği:
    {{
        ""ActorName"": ""Keanu Reeves"",
        ""DirectorName"": null,
        ""Category"": ""Action"",
        ""MinRating"": 7.0,
        ""MinYear"": 2010,
        ""SemanticSearch"": ""matrix dünyası""
    }}";
        }
    }
}