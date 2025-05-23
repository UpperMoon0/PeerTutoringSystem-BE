using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace PeerTutoringSystem.Api.Controllers.Booking
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TutorAvailabilityController : ControllerBase
    {
        private readonly ITutorAvailabilityService _availabilityService;
        private readonly ILogger<TutorAvailabilityController> _logger;

        public TutorAvailabilityController(ITutorAvailabilityService availabilityService, ILogger<TutorAvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> AddAvailability([FromBody] CreateTutorAvailabilityDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var availability = await _availabilityService.AddAsync(userId, dto);
                return Ok(new
                {
                    data = availability,
                    message = "Availability added successfully.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding tutor availability.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("tutor/{tutorId:guid}")]
        public async Task<IActionResult> GetTutorAvailability(Guid tutorId)
        {
            try
            {
                var availabilities = await _availabilityService.GetByTutorIdAsync(tutorId);
                return Ok(new
                {
                    data = availabilities,
                    count = availabilities.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving tutor availability for tutor {TutorId}.", tutorId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] Guid tutorId,
                                                         [FromQuery] DateTime startDate,
                                                         [FromQuery] DateTime endDate)
        {
            try
            {
                var currentDateTime = DateTime.UtcNow;
                if (startDate < currentDateTime)
                    return BadRequest(new { error = "Start date cannot be in the past.", timestamp = DateTime.UtcNow });

                if (endDate <= startDate)
                    return BadRequest(new { error = "End date must be after the start date.", timestamp = DateTime.UtcNow });

                var availabilities = await _availabilityService.GetAvailableSlotsAsync(tutorId, startDate, endDate);
                return Ok(new
                {
                    data = availabilities,
                    count = availabilities.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving available slots for tutor {TutorId}.", tutorId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        [HttpDelete("{availabilityId:guid}")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> DeleteAvailability(Guid availabilityId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var availability = await _availabilityService.GetByIdAsync(availabilityId);
                if (availability == null)
                    return NotFound(new { error = "Availability not found.", timestamp = DateTime.UtcNow });

                if (availability.TutorId != userId)
                    return StatusCode(403, new { error = "You can only delete your own availability slots.", timestamp = DateTime.UtcNow });

                var result = await _availabilityService.DeleteAsync(availabilityId);
                if (result)
                    return Ok(new { message = "Availability deleted successfully.", timestamp = DateTime.UtcNow });
                else
                    return StatusCode(500, new { error = "Failed to delete availability.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting availability {AvailabilityId}.", availabilityId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}