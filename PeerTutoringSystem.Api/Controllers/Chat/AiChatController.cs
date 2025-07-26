using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Chat
{
    public class AiChatRequest
    {
        public string Message { get; set; }
    }

    [ApiController]
    [Route("api/chat")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public AiChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost("ai-response")]
        public async Task<IActionResult> GetAiResponse([FromBody] AiChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }
            var response = await _aiChatService.GetAiResponse(request.Message);
            return Ok(response);
        }
    }
}