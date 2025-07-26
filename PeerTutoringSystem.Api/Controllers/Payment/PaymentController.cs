using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using PeerTutoringSystem.Application.Interfaces.Booking;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IBookingService bookingService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _configuration = configuration;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto request)
        {
            try
            {
                var result = await _paymentService.CreatePayment(request);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                // In a real app, log this exception
                return StatusCode(500, "An error occurred while creating the payment.");
            }
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