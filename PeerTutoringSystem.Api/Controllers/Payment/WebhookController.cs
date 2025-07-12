using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;

        public WebhookController(IConfiguration config, IPaymentService paymentService)
        {
            _config = config;
            _paymentService = paymentService;
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> HandleSePayWebhook([FromBody] SePayWebhookData webhookData)
        {
            try
            {
                if (webhookData == null)
                {
                    // Handle invalid or empty payload
                    return BadRequest(new { success = false, message = "Invalid payload." });
                }

                // Process the payment update
                await _paymentService.ProcessPaymentWebhook(webhookData);

                // Return 200 OK with { "success": true } as per SePay documentation
                // for API Key or No Authentication webhooks.
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}