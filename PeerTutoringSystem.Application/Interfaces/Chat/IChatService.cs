using PeerTutoringSystem.Domain.Entities.Chat;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Firebase.Database.Streaming;

namespace PeerTutoringSystem.Application.Interfaces.Chat
{
  public interface IChatService
  {
    Task SendMessageAsync(ChatMessage message);
    IObservable<ChatMessage> ObserveMessages();
  }
}