namespace PeerTutoringSystem.Application.Interfaces.Chat
{
    public interface IAiChatService
    {
        Task<string> GetAiResponse(string userMessage);
    }
}