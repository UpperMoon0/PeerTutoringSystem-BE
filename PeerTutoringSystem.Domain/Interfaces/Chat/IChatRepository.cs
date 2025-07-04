using PeerTutoringSystem.Domain.Entities.Chat;

namespace PeerTutoringSystem.Domain.Interfaces.Chat
{
    public interface IChatRepository
    {
        Task<ChatMessage> SendMessageAsync(ChatMessage message);
        Task<IEnumerable<Conversation>> GetConversationsAsync(string userId);
        Task<Conversation> FindOrCreateConversationAsync(string userId, string participantId);
    }
}