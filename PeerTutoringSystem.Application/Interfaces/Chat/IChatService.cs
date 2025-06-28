using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Chat
{
  public interface IChatService
  {
    Task SendMessageAsync(ChatMessage message);
  }
}