using PeerTutoringSystem.Domain.Entities.Chat;

namespace PeerTutoringSystem.Application.DTOs.Chat
{
    public class ConversationDto
    {
        public string Id { get; set; }
        public ConversationParticipantDto Participant { get; set; }
        public ChatMessage LastMessage { get; set; }
    }
}