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

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto request)
        {
            try
            {
                var result = await _paymentService.CreatePayment(request.BookingId, request.ReturnUrl);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                // In a real app, log this exception
                return StatusCode(500, "An error occurred while creating the payment.");
            }
        }
    }
}