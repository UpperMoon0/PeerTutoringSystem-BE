using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Api.Middleware;
using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.Interfaces.Profile_Bio;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Profile_Bio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserBioController : ControllerBase // Đổi tên controller
    {
        private readonly IUserBioService _profileService;

        public UserBioController(IUserBioService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> CreateUserBio([FromBody] CreateUserBioDto dto)
        {
            try
            {
                var tutorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var userBio = await _profileService.CreateProfileAsync(tutorId, dto);
                return Ok(userBio);
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

        [HttpGet("{bioId:int}")] 
        public async Task<IActionResult> GetUserBio(int bioId)
        {
            try
            {
                var userBio = await _profileService.GetProfileByIdAsync(bioId);
                return Ok(userBio);
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
        public async Task<IActionResult> GetUserBioByUserId(Guid userId)
        {
            try
            {
                var userBio = await _profileService.GetProfileByUserIdAsync(userId);
                return Ok(userBio);
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

        [HttpPut("{bioId:int}")] 
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> UpdateUserBio(int bioId, [FromBody] UpdateUserBioDto dto) 
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var userBio = await _profileService.GetProfileByIdAsync(bioId);

                // Chỉ Tutor sở hữu hồ sơ hoặc Admin mới được cập nhật
                var isAdmin = User.IsInRole("Admin");
                if (userBio.UserID != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to update this user bio." });

                await _profileService.UpdateProfileAsync(bioId, dto);
                return Ok(new { message = "User bio updated successfully." });
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