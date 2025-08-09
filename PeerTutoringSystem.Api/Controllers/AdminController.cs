using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPut("bookings/{bookingId}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(Guid bookingId, [FromBody] PaymentConfirmationDto paymentConfirmationDto)
        {
            var result = await _bookingService.ConfirmPayment(bookingId, paymentConfirmationDto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}