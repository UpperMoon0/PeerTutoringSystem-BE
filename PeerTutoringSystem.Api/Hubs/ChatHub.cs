using Microsoft.AspNetCore.SignalR;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Hubs
{
  public class ChatHub : Hub
  {
    public async Task SendMessage(ChatMessage message)
    {
      await Clients.All.SendAsync("ReceiveMessage", message);
    }
  }
}