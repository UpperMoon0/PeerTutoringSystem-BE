using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Chat
{
  [ApiController]
  [Route("api/[controller]")]
  public class ChatController : ControllerBase
  {
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
      _chatService = chatService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(ChatMessage message)
    {
      await _chatService.SendMessageAsync(message);
      return Ok();
    }
  }
}