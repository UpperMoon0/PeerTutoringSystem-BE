using PeerTutoringSystem.Application.DTOs.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;

namespace PeerTutoringSystem.Application.Interfaces.Chat
{
    public interface IChatService
    {
        Task<ChatMessage> SendMessageAsync(ChatMessage message);
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(string userId);
        Task<ConversationDto> FindOrCreateConversationAsync(string userId, string participantId);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(string conversationId);
        IObservable<ChatMessage> ObserveMessages();
    }
}