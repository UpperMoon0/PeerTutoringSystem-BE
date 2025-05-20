using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PeerTutoringSystem.Api.Controllers.Booking
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TutorAvailabilityController : ControllerBase
    {
        private readonly ITutorAvailabilityService _availabilityService;

        public TutorAvailabilityController(ITutorAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpPost]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> AddAvailability([FromBody] CreateTutorAvailabilityDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new ValidationException("Invalid token."));

                var availability = await _availabilityService.AddAsync(userId, dto);
                return Ok(availability);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("tutor/{tutorId:guid}")]
        public async Task<IActionResult> GetTutorAvailability(Guid tutorId)
        {
            try
            {
                var availabilities = await _availabilityService.GetByTutorIdAsync(tutorId);
                return Ok(availabilities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] Guid tutorId,
                                                         [FromQuery] DateTime startDate,
                                                         [FromQuery] DateTime endDate)
        {
            try
            {
                var availabilities = await _availabilityService.GetAvailableSlotsAsync(tutorId, startDate, endDate);
                return Ok(availabilities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
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
                    return NotFound(new { error = "Availability not found." });

                if (availability.TutorId != userId)
                    return StatusCode(403, new { error = "You can only delete your own availability slots." });

                var result = await _availabilityService.DeleteAsync(availabilityId);
                if (result)
                    return Ok(new { message = "Availability deleted successfully." });
                else
                    return StatusCode(500, new { error = "Failed to delete availability." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}
