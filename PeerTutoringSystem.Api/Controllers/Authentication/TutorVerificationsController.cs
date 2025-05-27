using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Api.Middleware;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PeerTutoringSystem.Api.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TutorVerificationsController : ControllerBase
    {
        private readonly ITutorVerificationService _tutorVerificationService;
        private readonly ILogger<TutorVerificationsController> _logger;

        public TutorVerificationsController(
            ITutorVerificationService tutorVerificationService,
            ILogger<TutorVerificationsController> logger)
        {
            _tutorVerificationService = tutorVerificationService;
            _logger = logger;
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAllVerifications()
        {
            try
            {
                _logger.LogInformation("Fetching all tutor verifications for admin.");
                var verifications = await _tutorVerificationService.GetAllVerificationsAsync();
                _logger.LogInformation("Successfully fetched {Count} tutor verifications.", verifications.Count());
                return Ok(verifications);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error while fetching all tutor verifications: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching all tutor verifications.");
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("{verificationId:guid}")]
        public async Task<IActionResult> GetVerification(Guid verificationId)
        {
            try
            {
                _logger.LogInformation("Fetching tutor verification with ID: {VerificationId}", verificationId);
                var verification = await _tutorVerificationService.GetVerificationByIdAsync(verificationId);
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");
                var isTutor = User.IsInRole("Tutor") && verification.UserID == currentUserId;

                if (!isAdmin && !isTutor)
                {
                    _logger.LogWarning("User ID: {UserId} does not have permission to view verification ID: {VerificationId}", currentUserId, verificationId);
                    return StatusCode(403, new { error = "You do not have permission to view this verification request." });
                }

                _logger.LogInformation("Successfully fetched tutor verification with ID: {VerificationId} for user ID: {UserId}", verificationId, currentUserId);
                return Ok(verification);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error while fetching tutor verification ID: {VerificationId}: {Error}", verificationId, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching tutor verification ID: {VerificationId}", verificationId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpPut("{verificationId:guid}")]
        [AuthorizeAdmin]
        public async Task<IActionResult> UpdateVerification(Guid verificationId, [FromBody] UpdateTutorVerificationDto dto)
        {
            try
            {
                _logger.LogInformation("Updating tutor verification with ID: {VerificationId}", verificationId);
                await _tutorVerificationService.UpdateVerificationAsync(verificationId, dto);
                _logger.LogInformation("Successfully updated tutor verification with ID: {VerificationId}", verificationId);
                return Ok(new { message = "Verification updated successfully." });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error while updating tutor verification ID: {VerificationId}: {Error}", verificationId, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating tutor verification ID: {VerificationId}", verificationId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("pending/{userId:guid}")]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> CheckPendingTutorVerification(Guid userId)
        {
            try
            {
                _logger.LogInformation("Checking pending or approved tutor verification for user ID: {UserId}", userId);
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));

                if (currentUserId != userId)
                {
                    _logger.LogWarning("User ID: {CurrentUserId} does not have permission to check pending verification for user ID: {UserId}", currentUserId, userId);
                    return StatusCode(403, new { error = "You do not have permission to check this user's verification status." });
                }

                var (hasVerification, latestStatus) = await _tutorVerificationService.HasPendingOrApprovedTutorVerificationAsync(userId, "Pending", "Approved");
                _logger.LogInformation("User ID: {UserId} has verification: {HasVerification}, latest status: {LatestStatus}", userId, hasVerification, latestStatus);
                return Ok(new { HasVerificationRequest = hasVerification, LatestStatus = latestStatus });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error while checking pending or approved tutor verification for user ID: {UserId}: {Error}", userId, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking pending or approved tutor verification for user ID: {UserId}", userId);
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}