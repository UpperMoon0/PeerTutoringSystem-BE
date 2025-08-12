using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Application.DTOs.Payment;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _paymentService.GetPaymentHistory(userId);
            if (!result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] PayOSCreatePaymentLinkRequestDto request)
        {
            try
            {
                var successUrl = _configuration["PayOS:SuccessUrl"];
                var cancelUrl = _configuration["PayOS:CancelUrl"];
                var result = await _paymentService.CreatePaymentLink(request, successUrl, cancelUrl);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("transaction-history")]
        public async Task<IActionResult> GetTransactionHistory()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _paymentService.GetTransactionHistory(userId);
            return Ok(result);
        }

        [HttpGet("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] Guid bookingId)
        {
            var result = await _paymentService.ConfirmPayment(bookingId);
            return Ok(new { success = result });
        }
    }
}