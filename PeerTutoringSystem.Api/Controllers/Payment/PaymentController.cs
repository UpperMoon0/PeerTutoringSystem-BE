using Microsoft.AspNetCore.Mvc;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var checkoutUrl = await _paymentService.CreatePaymentLink(
                    request.OrderCode,
                    request.Amount,
                    request.Description,
                    request.ReturnUrl,
                    request.CancelUrl
                );
                return Ok(new { checkoutUrl });
            }
            catch (System.Exception ex)
            {
                // In a real app, log this exception
                return StatusCode(500, "An error occurred while creating the payment link.");
            }
        }
    }

    public record CreatePaymentRequest(int OrderCode, int Amount, string Description, string ReturnUrl, string CancelUrl);
}