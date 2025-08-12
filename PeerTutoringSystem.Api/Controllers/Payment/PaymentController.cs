using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.DTOs.Booking;

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

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _paymentService.GetPaymentHistory(userId);
            if (!result.Any())
            {
                return Ok(new List<object>());
            }
            return Ok(result);
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] PayOSCreatePaymentLinkRequestDto request)
        {
            try
            {
                var result = await _paymentService.CreatePaymentLink(request, request.returnUrl, request.cancelUrl);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] Guid bookingId)
        {
            var result = await _paymentService.ConfirmPayment(bookingId);
            return Ok(new { success = result });
        }

        [HttpPost("payos-webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookData webhookData)
        {
            try
            {
                await _paymentService.HandlePayOSWebhook(webhookData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("payment-return")]
        public async Task<ActionResult<BookingSessionDto>> PaymentReturn([FromQuery] long orderCode)
        {
            var result = await _paymentService.HandlePayOSReturn(orderCode);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("payment-cancel")]
        public async Task<ActionResult<BookingSessionDto>> PaymentCancel([FromQuery] long orderCode)
        {
            var result = await _paymentService.HandlePayOSCancel(orderCode);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}