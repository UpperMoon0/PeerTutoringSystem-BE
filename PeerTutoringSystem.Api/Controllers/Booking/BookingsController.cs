using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using PeerTutoringSystem.Domain.Entities.Booking;

namespace PeerTutoringSystem.Api.Controllers.Booking
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var booking = await _bookingService.CreateBookingAsync(studentId, dto);
                return Ok(booking);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating booking.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentBookings()
        {
            try
            {
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var bookings = await _bookingService.GetBookingsByStudentAsync(studentId);
                return Ok(bookings);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving student bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("tutor")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> GetTutorBookings()
        {
            try
            {
                var tutorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var bookings = await _bookingService.GetBookingsByTutorAsync(tutorId);
                return Ok(bookings);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving tutor bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingBookings()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));
                var isTutor = User.IsInRole("Tutor");

                var bookings = await _bookingService.GetUpcomingBookingsAsync(userId, isTutor);
                return Ok(bookings);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving upcoming bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("{bookingId:guid}")]
        public async Task<IActionResult> GetBooking(Guid bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return NotFound(new { error = "Booking not found." });

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");

                if (booking.StudentId != userId && booking.TutorId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to view this booking." });

                return Ok(booking);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving booking {BookingId}.", bookingId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpPut("{bookingId:guid}/status")]
        public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusDto dto)
        {
            try
            {
                // Validate the status field
                if (string.IsNullOrEmpty(dto.Status) || !Enum.TryParse<BookingStatus>(dto.Status, true, out _))
                {
                    return BadRequest(new { error = "Invalid booking status. Valid values are: Pending, Confirmed, Completed, Cancelled." });
                }

                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return NotFound(new { error = "Booking not found." });

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");

                // Student can only cancel their own bookings
                if (dto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && booking.StudentId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to cancel this booking." });

                // Tutor can only confirm or complete bookings
                if ((dto.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
                     dto.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)) &&
                    booking.TutorId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to update this booking status." });

                var updatedBooking = await _bookingService.UpdateBookingStatusAsync(bookingId, dto);
                return Ok(updatedBooking);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating booking status for booking {BookingId}.", bookingId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}