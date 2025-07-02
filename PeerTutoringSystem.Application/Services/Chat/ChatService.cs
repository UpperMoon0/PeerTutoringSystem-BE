using Firebase.Database;
using Firebase.Database.Query;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Firebase.Database.Streaming;

namespace PeerTutoringSystem.Application.Services.Chat
{
  public class ChatService : IChatService
  {
    private readonly FirebaseClient _firebaseClient;

    public ChatService(FirebaseClient firebaseClient)
    {
      _firebaseClient = firebaseClient;
    }

    public async Task SendMessageAsync(ChatMessage message)
    {
      await _firebaseClient
          .Child("chats")
          .PostAsync(message);
    }

    public IObservable<ChatMessage> ObserveMessages()
    {
      return _firebaseClient
          .Child("chats")
          .AsObservable<ChatMessage>()
          .Select(fbe => fbe.Object);
    }
  }
}