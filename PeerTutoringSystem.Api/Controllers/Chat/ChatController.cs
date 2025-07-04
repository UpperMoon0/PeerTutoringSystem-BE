using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PeerTutoringSystem.Api.Hubs;
using PeerTutoringSystem.Application.DTOs.Chat;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Security.Claims;

namespace PeerTutoringSystem.Api.Controllers.Chat
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
        {
            var message = new ChatMessage
            {
                SenderId = messageDto.SenderId,
                ReceiverId = messageDto.ReceiverId,
                Message = messageDto.Message,
            };
            var sentMessage = await _chatService.SendMessageAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", sentMessage);
            return Ok(sentMessage);
        }

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversations(string userId)
        {
            var conversations = await _chatService.GetConversationsAsync(userId);
            return Ok(conversations);
        }
        [HttpPost("find-or-create")]
        public async Task<IActionResult> FindOrCreateConversation([FromBody] FindOrCreateConversationRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(request.ParticipantId))
            {
                return BadRequest("User ID and participant ID must be provided.");
            }
            var conversation = await _chatService.FindOrCreateConversationAsync(userId, request.ParticipantId);
            return Ok(conversation);
        }
    }
}