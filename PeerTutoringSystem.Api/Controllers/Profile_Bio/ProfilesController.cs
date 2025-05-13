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

namespace PeerTutoringSystem.Api.Controllers.Profile_Bio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
        {
            try
            {
                var tutorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var profile = await _profileService.CreateProfileAsync(tutorId, dto);
                return Ok(profile);
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

        [HttpGet("{profileId:int}")]
        public async Task<IActionResult> GetProfile(int profileId)
        {
            try
            {
                var profile = await _profileService.GetProfileByIdAsync(profileId);
                return Ok(profile);
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

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetProfileByUserId(Guid userId)
        {
            try
            {
                var profile = await _profileService.GetProfileByUserIdAsync(userId);
                return Ok(profile);
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

        [HttpPut("{profileId:int}")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> UpdateProfile(int profileId, [FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var profile = await _profileService.GetProfileByIdAsync(profileId);

                // Chỉ Tutor sở hữu hồ sơ hoặc Admin mới được cập nhật
                var isAdmin = User.IsInRole("Admin");
                if (profile.UserID != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to update this profile." });

                await _profileService.UpdateProfileAsync(profileId, dto);
                return Ok(new { message = "Profile updated successfully." });
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