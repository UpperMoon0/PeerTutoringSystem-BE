using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Application.Services.Payment;

namespace PeerTutoringSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var result = await _paymentService.GetPaymentHistory();
            if (!result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}