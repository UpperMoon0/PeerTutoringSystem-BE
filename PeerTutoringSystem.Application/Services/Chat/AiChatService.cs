using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Reviews;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PeerTutoringSystem.Application.Services.Chat
{
    public class AiChatService : IAiChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IChatRepository _chatRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly IUserBioRepository _userBioRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IUserSkillRepository _userSkillRepository;
        private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
        private readonly IBookingSessionRepository _bookingSessionRepository;
        private readonly IReviewRepository _reviewRepository;
        private const string AiUserId = "00000000-0000-0000-0000-000000000000";

        public AiChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IChatRepository chatRepository, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, IUserBioRepository userBioRepository, ISkillRepository skillRepository, IUserSkillRepository userSkillRepository, ITutorAvailabilityRepository tutorAvailabilityRepository, IBookingSessionRepository bookingSessionRepository, IReviewRepository reviewRepository)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _chatRepository = chatRepository;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _userBioRepository = userBioRepository;
            _skillRepository = skillRepository;
            _userSkillRepository = userSkillRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _bookingSessionRepository = bookingSessionRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<ChatMessage> GenerateResponseAsync(string userMessage)
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

            var tutorContext = await GetTutorRecommendationContextAsync();
            var instruction = "You are a professional AI assistant for a peer tutoring system. Your role is to help users with their questions about the system, subjects, and tutoring. Be helpful, friendly, and professional. You can also recommend tutors based on the following information. If no tutors are available, inform the user and offer to help with other questions.";
            var history = chatHistory.Messages.TakeLast(20).Select(m => $"{(m.SenderId == userId ? "user" : "ai")}: {m.Message}");
            var prompt = $"{instruction}\n\nTutor Information:\n{tutorContext}\n\nConversation History:\n{string.Join("\n", history)}";

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

        private async Task<string> GetTutorRecommendationContextAsync()
        {
            var tutors = await _userRepository.GetUsersByRoleAsync("Tutor");
            if (!tutors.Any())
            {
                return "No tutors are currently available.";
            }

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("Here is a list of available tutors:");

            foreach (var tutor in tutors)
            {
                var userBio = await _userBioRepository.GetByUserIdAsync(tutor.UserID);
                var userSkills = await _userSkillRepository.GetByUserIdAsync(tutor.UserID);
                var tutorAvailabilities = await _tutorAvailabilityRepository.GetByTutorIdAsync(tutor.UserID);
                var tutorReviews = await _reviewRepository.GetByTutorIdAsync(tutor.UserID);

                contextBuilder.AppendLine($"\nTutor: {tutor.FullName}");
                if (userBio != null)
                {
                    contextBuilder.AppendLine($"  Bio: {userBio.Bio}");
                }

                if (userSkills.Any())
                {
                    var skills = await _skillRepository.GetAllAsync();
                    var skillNames = userSkills.Select(us => skills.FirstOrDefault(s => s.SkillID == us.SkillID)?.SkillName);
                    contextBuilder.AppendLine($"  Skills: {string.Join(", ", skillNames.Where(sn => !string.IsNullOrEmpty(sn)))}");
                }

                if (tutorAvailabilities.Any())
                {
                    contextBuilder.AppendLine("  Availability:");
                    foreach (var availability in tutorAvailabilities)
                    {
                        if (availability.IsRecurring && availability.RecurringDay.HasValue)
                        {
                            contextBuilder.AppendLine($"    - {availability.RecurringDay.Value}: {availability.StartTime:hh\\:mm} - {availability.EndTime:hh\\:mm}");
                        }
                        else
                        {
                            contextBuilder.AppendLine($"    - {availability.StartTime:dd/MM/yyyy hh\\:mm} - {availability.EndTime:hh\\:mm}");
                        }
                    }
                }

                if (tutorReviews.Any())
                {
                    var averageRating = tutorReviews.Average(r => r.Rating);
                    contextBuilder.AppendLine($"  Average Rating: {averageRating:F1}/5.0 from {tutorReviews.Count()} reviews");
                }
            }

            return contextBuilder.ToString();
        }
    }
}