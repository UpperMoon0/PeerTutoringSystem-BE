using System;

namespace PeerTutoringSystem.Domain.Entities.Chat
{
  public class ChatMessage
  {
    public Guid Id { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
  }
}