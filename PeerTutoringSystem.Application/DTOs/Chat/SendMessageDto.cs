using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs.Chat
{
    public class SendMessageDto
    {
        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}