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
            try
            {
                var result = await _payOSService.CreatePaymentLink(request, request.returnUrl, request.cancelUrl);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}