using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Api.Hubs;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace PeerTutoringSystem.Api.Controllers.Chat
{
  [ApiController]
  [Route("api/[controller]")]
  public class ChatController : ControllerBase
  {
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
      _chatService = chatService;
      _hubContext = hubContext;
      _chatService.ObserveMessages().Subscribe(async message =>
      {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
      });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(ChatMessage message)
    {
      await _chatService.SendMessageAsync(message);
      return Ok();
    }
    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
      // This endpoint is primarily for initial message retrieval or historical data.
      // Real-time updates are handled via SignalR.
      // You might want to implement a method in ChatService to fetch a limited history of messages here.
      return Ok("Real-time messages are streamed via SignalR. This endpoint can be used for historical data if implemented.");
    }
  }
}