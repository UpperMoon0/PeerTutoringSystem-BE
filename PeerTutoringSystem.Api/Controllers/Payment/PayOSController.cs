using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Application.DTOs.Payment;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayOSController : ControllerBase
    {
        private readonly IPayOSService _payOSService;

        public PayOSController(IPayOSService payOSService)
        {
            _payOSService = payOSService;
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] PayOSCreatePaymentLinkRequestDto request)
        {
            var result = await _payOSService.CreatePaymentLink(request);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest("Failed to create payment link.");
        }
    }
}