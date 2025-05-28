using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PeerTutoringSystem.Application.DTOs.Booking;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var session = await _sessionService.CreateSessionAsync(userId, dto);
                return Ok(new { data = session, message = "Session created successfully.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating session.");
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating session.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{sessionId:guid}")]
        [Authorize(Roles = "Student,Tutor,Admin")]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            try
            {
                var session = await _sessionService.GetSessionByIdAsync(sessionId);
                if (session == null)
                {
                    return NotFound(new { error = "Session not found.", timestamp = DateTime.UtcNow });
                }

                return Ok(new { data = session, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving session {SessionId}.", sessionId);
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserSessions([FromQuery] BookingFilterDto filter)
        {
            if (filter == null || filter.Page < 1 || filter.PageSize < 1)
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
                var (sessions, totalCount) = await _sessionService.GetSessionsByUserAsync(userId, isTutor, filter);
                return Ok(new
                {
                    data = sessions,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving user sessions.");
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpPut("{sessionId:guid}")]
        [Authorize(Roles = "Tutor,Admin")]
        public async Task<IActionResult> UpdateSession(Guid sessionId, [FromBody] UpdateSessionDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
            }

            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return BadRequest(new { error = "Invalid user token.", timestamp = DateTime.UtcNow });
                }

                var session = await _sessionService.UpdateSessionAsync(sessionId, dto);
                return Ok(new { data = session, message = "Session updated successfully.", timestamp = DateTime.UtcNow });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating session {SessionId}.", sessionId);
                return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating session {SessionId}.", sessionId);
                return StatusCode(500, new { error = "An unexpected error occurred.", timestamp = DateTime.UtcNow });
            }
        }
    }
}