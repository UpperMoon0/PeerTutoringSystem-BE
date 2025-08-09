using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Payment;

namespace PeerTutoringSystem.Api.Controllers.Booking
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("{bookingId}/upload-proof")]
        public async Task<IActionResult> UploadProofOfPayment(Guid bookingId, IFormFile file)
        {
            var result = await _bookingService.UploadProofOfPayment(bookingId, file);
            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}