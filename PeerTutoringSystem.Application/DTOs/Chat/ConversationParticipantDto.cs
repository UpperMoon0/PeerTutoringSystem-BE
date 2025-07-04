namespace PeerTutoringSystem.Application.DTOs.Chat
{
    public class ConversationParticipantDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}