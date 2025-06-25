using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using PeerTutoringSystem.Domain.Entities.Booking;
using System;

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
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var studentId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var booking = await _bookingService.CreateBookingAsync(studentId, dto);
                return Ok(new { data = booking, message = "Booking created successfully.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating booking for student.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating booking for student.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpPost("instant")]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> CreateInstantBooking([FromBody] InstantBookingDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var studentId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var booking = await _bookingService.CreateInstantBookingAsync(studentId, dto);
                return Ok(new { data = booking, message = "Instant booking created successfully.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating instant booking.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating instant booking.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> GetStudentBookings([FromQuery] BookingFilterDto filter)
        {
            // Initialize default filter if none provided
            filter ??= new BookingFilterDto();

            // Ensure valid pagination parameters
            if (filter.Page < 1 || filter.PageSize < 1)
            {
                return BadRequest(new { error = "Invalid pagination parameters.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var studentId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

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
                _logger.LogWarning(ex, "Validation error while retrieving student bookings.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving student bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("tutor")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> GetTutorBookings([FromQuery] BookingFilterDto filter)
        {
            // Initialize default filter if none provided
            filter ??= new BookingFilterDto();

            // Ensure valid pagination parameters
            if (filter.Page < 1 || filter.PageSize < 1)
            {
                return BadRequest(new { error = "Invalid pagination parameters.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var tutorId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

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
                _logger.LogWarning(ex, "Validation error while retrieving tutor bookings.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving tutor bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("upcoming")]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> GetUpcomingBookings([FromQuery] BookingFilterDto filter)
        {
            // Initialize default filter if none provided
            filter ??= new BookingFilterDto();

            // Ensure valid pagination parameters
            if (filter.Page < 1 || filter.PageSize < 1)
            {
                return BadRequest(new { error = "Invalid pagination parameters.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

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
                _logger.LogWarning(ex, "Validation error while retrieving upcoming bookings.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving upcoming bookings.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{bookingId:guid}")]
        [Authorize(Roles = "Student,Tutor,Admin")]
        public async Task<IActionResult> GetBooking(Guid bookingId)
        {
            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                {
                    return NotFound(new { error = "Booking not found.", timestamp = DateTime.UtcNow });
                }

                var isAdmin = User.IsInRole("Admin");
                if (booking.StudentId != userId && booking.TutorId != userId && !isAdmin)
                {
                    return StatusCode(403, new { error = "You do not have permission to view this booking.", timestamp = DateTime.UtcNow });
                }

                return Ok(new { data = booking, timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while retrieving booking {BookingId}.", bookingId);
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving booking {BookingId}.", bookingId);
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpPut("{bookingId:guid}/status")]
        [Authorize(Roles = "Student,Tutor,Admin")]
        public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status?.Trim()))
            {
                return BadRequest(new { error = "Status is required.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var statusString = dto.Status.Trim();
                if (!Enum.TryParse<BookingStatus>(statusString, true, out var status))
                {
                    return BadRequest(new
                    {
                        error = $"Invalid booking status. Valid values are: Pending, Confirmed, Completed, Cancelled, Rejected. Example: 'Pending'.",
                        timestamp = DateTime.UtcNow
                    });
                }

                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                {
                    return NotFound(new { error = "Booking not found.", timestamp = DateTime.UtcNow });
                }

                // Parse booking.Status (string) to BookingStatus enum
                if (!Enum.TryParse<BookingStatus>(booking.Status, true, out var currentBookingStatus))
                {
                    return BadRequest(new { error = "Invalid current booking status.", timestamp = DateTime.UtcNow });
                }

                var isAdmin = User.IsInRole("Admin");

                switch (status)
                {
                    case BookingStatus.Cancelled:
                        if (booking.StudentId != userId && !isAdmin)
                        {
                            return StatusCode(403, new { error = "Only the student or admin can cancel this booking.", timestamp = DateTime.UtcNow });
                        }
                        break;

                    case BookingStatus.Confirmed:
                    case BookingStatus.Completed:
                        if (booking.TutorId != userId && !isAdmin)
                        {
                            return StatusCode(403, new { error = "Only the assigned tutor or admin can update this booking status.", timestamp = DateTime.UtcNow });
                        }
                        break;

                    case BookingStatus.Rejected:
                        if (booking.TutorId != userId && !isAdmin)
                        {
                            return StatusCode(403, new { error = "Only the assigned tutor or admin can reject this booking.", timestamp = DateTime.UtcNow });
                        }
                        if (currentBookingStatus != BookingStatus.Pending)
                        {
                            return BadRequest(new { error = "Only bookings in Pending status can be rejected.", timestamp = DateTime.UtcNow });
                        }
                        break;

                    default:
                        break;
                }

                if (currentBookingStatus == status)
                {
                    return BadRequest(new { error = "The booking already has this status.", timestamp = DateTime.UtcNow });
                }

                var updatedBooking = await _bookingService.UpdateBookingStatusAsync(bookingId, dto);

                return Ok(new
                {
                    data = updatedBooking,
                    message = "Booking status updated successfully.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating booking status for booking {BookingId}.", bookingId);
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while updating booking status for booking {BookingId}.", bookingId);
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating booking status for booking {BookingId}.", bookingId);
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings([FromQuery] BookingFilterDto filter)
        {
            // Initialize default filter if none provided
            filter ??= new BookingFilterDto();

            // Ensure valid pagination parameters
            if (filter.Page < 1 || filter.PageSize < 1)
            {
                return BadRequest(new { error = "Invalid pagination parameters.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var (bookings, totalCount) = await _bookingService.GetAllBookingsForAdminAsync(filter);
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
                _logger.LogWarning(ex, "Validation error while retrieving all bookings for admin.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving all bookings for admin.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("tutor/dashboard-stats")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> GetTutorDashboardStats()
        {
            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var tutorId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var stats = await _bookingService.GetTutorDashboardStatsAsync(tutorId);
                return Ok(new
                {
                    data = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving tutor dashboard stats.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }
    }
}