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
                return Ok(new { data = booking, message = "Booking created successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating booking.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("instant")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateInstantBooking([FromBody] InstantBookingDto dto)
        {
            try
            {
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var booking = await _bookingService.CreateInstantBookingAsync(studentId, dto);
                return Ok(new { data = booking, message = "Instant booking created successfully.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating instant booking.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentBookings([FromQuery] BookingFilterDto filter)
        {
            try
            {
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var (bookings, totalCount) = await _bookingService.GetBookingsByStudentAsync(studentId, filter);
                return Ok(new
                {
                    data = bookings,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving student bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("tutor")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> GetTutorBookings([FromQuery] BookingFilterDto filter)
        {
            try
            {
                var tutorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var (bookings, totalCount) = await _bookingService.GetBookingsByTutorAsync(tutorId, filter);
                return Ok(new
                {
                    data = bookings,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving tutor bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingBookings([FromQuery] BookingFilterDto filter)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));
                var isTutor = User.IsInRole("Tutor");

                var (bookings, totalCount) = await _bookingService.GetUpcomingBookingsAsync(userId, isTutor, filter);
                return Ok(new
                {
                    data = bookings,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving upcoming bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{bookingId:guid}")]
        public async Task<IActionResult> GetBooking(Guid bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return NotFound(new { error = "Booking not found.", timestamp = DateTime.UtcNow });

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");

                if (booking.StudentId != userId && booking.TutorId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to view this booking.", timestamp = DateTime.UtcNow });

                return Ok(new { data = booking, timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving booking {BookingId}.", bookingId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpPut("{bookingId:guid}/status")]
        public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Status) || !Enum.TryParse<BookingStatus>(dto.Status, true, out _))
                {
                    return BadRequest(new { error = "Invalid booking status. Valid values are: Pending, Confirmed, Completed, Cancelled.", timestamp = DateTime.UtcNow });
                }

                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return NotFound(new { error = "Booking not found.", timestamp = DateTime.UtcNow });

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");

                if (dto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && booking.StudentId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to cancel this booking.", timestamp = DateTime.UtcNow });

                if ((dto.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
                     dto.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)) &&
                    booking.TutorId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to update this booking status.", timestamp = DateTime.UtcNow });

                var updatedBooking = await _bookingService.UpdateBookingStatusAsync(bookingId, dto);
                return Ok(new { data = updatedBooking, message = "Booking status updated successfully.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating booking status for booking {BookingId}.", bookingId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}