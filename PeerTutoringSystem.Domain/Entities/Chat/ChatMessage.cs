using System;

namespace PeerTutoringSystem.Domain.Entities.Chat
{
    public class ChatMessage
    {
        public Guid MessageId { get; set; }
        public string? Message { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}