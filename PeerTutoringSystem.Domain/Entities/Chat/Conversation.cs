using PeerTutoringSystem.Domain.Entities.Authentication;
using System.Text.Json.Serialization;

namespace PeerTutoringSystem.Domain.Entities.Chat
{
    public class Conversation
    {
        public string? Id { get; set; }
        public List<string>? ParticipantIds { get; set; }
        public ChatMessage? LastMessage { get; set; }
        [JsonIgnore]
        public User? Participant { get; set; }
    }
}