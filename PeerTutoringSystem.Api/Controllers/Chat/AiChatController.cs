using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Chat
{
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
        public async Task<IActionResult> GetAiResponse([FromBody] string userMessage)
        {
            var response = await _aiChatService.GetAiResponse(userMessage);
            return Ok(response);
        }
    }
}