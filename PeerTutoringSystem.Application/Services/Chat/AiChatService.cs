using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Chat
{
    public class AiChatService : IAiChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IChatRepository _chatRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string AiUserId = "00000000-0000-0000-0000-000000000000";

        public AiChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IChatRepository chatRepository, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _chatRepository = chatRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ChatMessage> GetAiResponse(string userMessage)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return new ChatMessage { Message = "User not authenticated.", SenderId = "ai", ReceiverId = "user", Timestamp = DateTime.UtcNow };
            }

            var conversation = await _chatRepository.FindOrCreateConversationAsync(userId, AiUserId);
            var historyMessages = await _chatRepository.GetMessagesAsync(conversation.Id);

            var chatHistory = new ChatHistory
            {
                Messages = historyMessages.ToList()
            };

            var userChatMessage = new ChatMessage { Message = userMessage, SenderId = userId, ReceiverId = AiUserId, Timestamp = DateTime.UtcNow };
            await _chatRepository.SendMessageAsync(userChatMessage);
            chatHistory.Messages.Add(userChatMessage);

            var apiKey = _configuration["GEMINI_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return new ChatMessage { Message = "API key not configured.", SenderId = AiUserId, ReceiverId = userId, Timestamp = DateTime.UtcNow };
            }

            var instruction = "You are a professional AI assistant for a peer tutoring system. Your role is to help users with their questions about the system, subjects, and tutoring. Be helpful, friendly, and professional.";
            var history = chatHistory.Messages.TakeLast(20).Select(m => $"{(m.SenderId == userId ? "user" : "ai")}: {m.Message}");
            var prompt = $"{instruction}\n\n{string.Join("\n", history)}";

            var client = _httpClientFactory.CreateClient();
            var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
                var text = jsonResponse.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                var aiMessage = new ChatMessage { Message = text?.Trim() ?? "No response from AI.", SenderId = AiUserId, ReceiverId = userId, Timestamp = DateTime.UtcNow };
                await _chatRepository.SendMessageAsync(aiMessage);
                return aiMessage;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new ChatMessage { Message = $"Error from AI service: {response.StatusCode}, {error}", SenderId = AiUserId, ReceiverId = userId, Timestamp = DateTime.UtcNow };
            }
        }
    }
}