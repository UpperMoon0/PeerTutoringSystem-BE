using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Chat
{
    public interface IAiChatService
    {
        Task<ChatMessage> GenerateResponseAsync(string userMessage);
    }
}