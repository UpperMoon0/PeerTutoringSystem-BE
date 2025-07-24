using PeerTutoringSystem.Application.Interfaces.Chat;

namespace PeerTutoringSystem.Application.Services.Chat
{
    public class AiChatService : IAiChatService
    {
        public Task<string> GetAiResponse(string userMessage)
        {
            return Task.FromResult("This is a response from the AI.");
        }
    }
}