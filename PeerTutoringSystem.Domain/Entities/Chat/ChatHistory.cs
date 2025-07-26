using System.Collections.Generic;

namespace PeerTutoringSystem.Domain.Entities.Chat
{
    public class ChatHistory
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}