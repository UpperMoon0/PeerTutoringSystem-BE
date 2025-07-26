using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.Interfaces.Chat;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Chat
{
    public class AiChatService : IAiChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AiChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string> GetAiResponse(string userMessage)
        {
            var apiKey = _configuration["GEMINI_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return "API key not configured.";
            }

            var client = _httpClientFactory.CreateClient();
            var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = userMessage
                            }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
                var candidates = jsonResponse.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    var contentProperty = firstCandidate.GetProperty("content");
                    var parts = contentProperty.GetProperty("parts");
                    if (parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString()?.Trim() ?? "No response from AI.";
                    }
                }
                return "No response from AI.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error from AI service: {response.StatusCode}, {error}";
            }
        }
    }
}