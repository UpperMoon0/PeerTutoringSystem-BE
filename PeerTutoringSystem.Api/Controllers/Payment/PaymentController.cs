using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IVietQrService _vietQrService;

        public PaymentController(IPaymentService paymentService, IVietQrService vietQrService)
        {
            _paymentService = paymentService;
            _vietQrService = vietQrService;
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

        [HttpPost("generate-qr")]
        public async Task<IActionResult> GenerateQrCode([FromBody] VietQrRequestDto request)
        {
            var result = await _vietQrService.GenerateQrCode(request);
            if (result?.data?.qrDataURL != null)
            {
                return Ok(new { qrDataURL = result.data.qrDataURL });
            }
            return BadRequest("Could not generate QR code.");
        }
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto request)
        {
            var result = await _paymentService.ConfirmPayment(request.BookingId);
            if (result)
            {
                return Ok(new { message = "Payment confirmed successfully." });
            }
            return BadRequest(new { message = "Payment confirmation failed." });
        }

        [HttpGet("admin/finance-details")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminFinanceDetails()
        {
            var result = await _paymentService.GetAdminFinanceDetails();
            return Ok(result);
        }
    }
}