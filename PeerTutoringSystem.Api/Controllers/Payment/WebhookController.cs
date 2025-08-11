using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IPayOSService payOSService, ILogger<WebhookController> logger)
        {
            _payOSService = payOSService;
            _logger = logger;
        }

        [HttpPost("payos")]
        public async Task<IActionResult> HandlePayOSWebhook([FromBody] PayOSWebhookData data)
        {
            if (data == null)
            {
                return BadRequest(new { success = false, message = "Invalid payload." });
            }

            try
            {
                await _payOSService.ProcessPayOSWebhook(data);
                return Ok(new { success = true });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook.");
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}