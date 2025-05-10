using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Api.Middleware;
using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TutorVerificationsController : ControllerBase
    {
        private readonly ITutorVerificationService _tutorVerificationService;

        public TutorVerificationsController(ITutorVerificationService tutorVerificationService)
        {
            _tutorVerificationService = tutorVerificationService;
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAllVerifications()
        {
            try
            {
                var verifications = await _tutorVerificationService.GetAllVerificationsAsync();
                return Ok(verifications);
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

        [HttpGet("{verificationId:guid}")]
        public async Task<IActionResult> GetVerification(Guid verificationId)
        {
            try
            {
                var verification = await _tutorVerificationService.GetVerificationByIdAsync(verificationId);
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");
                var isTutor = User.IsInRole("Tutor") && verification.UserID == currentUserId;

                if (!isAdmin && !isTutor)
                    return StatusCode(403, new { error = "You do not have permission to view this verification request." });

                return Ok(verification);
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

        [HttpPut("{verificationId:guid}")]
        [AuthorizeAdmin]
        public async Task<IActionResult> UpdateVerification(Guid verificationId, [FromBody] UpdateTutorVerificationDto dto)
        {
            try
            {
                await _tutorVerificationService.UpdateVerificationAsync(verificationId, dto);
                return Ok(new { message = "Verification updated successfully." });
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