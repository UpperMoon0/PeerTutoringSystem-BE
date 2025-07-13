using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using System;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IUserBioRepository _userBioRepository;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger, IBookingService bookingService, IUserBioRepository userBioRepository)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _userBioRepository = userBioRepository ?? throw new ArgumentNullException(nameof(userBioRepository));
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
                }

                var response = await _paymentService.CreatePayment(dto.BookingId, dto.ReturnUrl);
                
                if (!response.Success)
                {
                    return BadRequest(new { error = response.Message, timestamp = DateTime.UtcNow });
                }

                return Ok(new
                {
                    data = response,
                    message = "PaymentEntity created successfully.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, new { error = "An error occurred while creating payment.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{paymentId}")]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            try
            {
                var status = await _paymentService.GetPaymentStatus(paymentId);
                
                return Ok(new 
                { 
                    data = new { status = status.ToString() },
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for payment ID {PaymentId}", paymentId);
                return StatusCode(500, new { error = "An error occurred while getting payment status.", timestamp = DateTime.UtcNow });
            }
        }
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookData webhookData)
        {
            try
            {
                _logger.LogInformation("SePay Webhook received: {@WebhookData}", webhookData);
                await _paymentService.ProcessPaymentWebhook(webhookData);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay webhook.");
                return StatusCode(500, new { error = "An error occurred while processing the webhook." });
            }
   
        }

        // Endpoint for simulated payment
        [HttpPost("process")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
                }

                var booking = await _bookingService.GetBookingByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    return NotFound(new { error = "Booking not found.", timestamp = DateTime.UtcNow });
                }

                var tutorBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
                if (tutorBio == null)
                {
                    return NotFound(new { error = "Tutor bio not found.", timestamp = DateTime.UtcNow });
                }

                var durationInHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours;
                var calculatedAmount = durationInHours * tutorBio.HourlyRate;

                if (dto.Amount != calculatedAmount)
                {
                    return BadRequest(new { error = "Invalid payment amount.", timestamp = DateTime.UtcNow });
                }

                if (booking.Status == "Confirmed" && booking.PaymentStatus != "Paid")
                {
                    await _bookingService.UpdateBookingStatusAsync(dto.BookingId, new Application.DTOs.Booking.UpdateBookingStatusDto { PaymentStatus = "Paid" });
                }
                else if (booking.Status == "Pending")
                {
                    await _bookingService.UpdateBookingStatusAsync(dto.BookingId, new Application.DTOs.Booking.UpdateBookingStatusDto { Status = "Confirmed", PaymentStatus = "Paid" });
                }

                return Ok(new
                {
                    message = "Payment processed successfully.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for booking {BookingId}", dto.BookingId);
                return StatusCode(500, new { error = "An error occurred while processing payment.", timestamp = DateTime.UtcNow });
            }
        }
    }
}