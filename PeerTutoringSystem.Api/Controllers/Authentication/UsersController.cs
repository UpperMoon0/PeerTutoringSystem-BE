using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Api.Middleware;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITutorVerificationService _tutorVerificationService;
        private readonly ISkillService _skillService;

        public UsersController(IUserService userService, ITutorVerificationService tutorVerificationService, ISkillService skillService)
        {
            _userService = userService;
            _tutorVerificationService = tutorVerificationService;
            _skillService = skillService;
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
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

        [HttpGet("tutors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTutors()
        {
            try
            {
                var tutors = await _userService.GetAllTutorsAsync();
                return Ok(tutors);
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

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");
                if (currentUserId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to access this user's information." });

                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(user);
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

        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromForm] UpdateUserDto dto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var isAdmin = User.IsInRole("Admin");
                if (currentUserId != userId && !isAdmin)
                    return StatusCode(403, new { error = "You do not have permission to update this user's information." });

                await _userService.UpdateUserAsync(userId, dto);
                return Ok(new { message = "User updated successfully." });
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

        [HttpDelete("{userId:guid}")]
        [AuthorizeAdmin]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                await _userService.BanUserAsync(userId);
                return Ok(new { message = "User banned successfully." });
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

        [HttpPost("{userId:guid}/unban")]
        [AuthorizeAdmin]
        public async Task<IActionResult> UnbanUser(Guid userId)
        {
            try
            {
                await _userService.UnbanUserAsync(userId);
                return Ok(new { message = "User unbanned successfully." });
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

        [HttpPost("{userId:guid}/request-tutor")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestTutor(Guid userId, [FromBody] RequestTutorDto dto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                if (currentUserId != userId)
                    return StatusCode(403, new { error = "You do not have permission to request tutor role for this user." });

                var verificationId = await _tutorVerificationService.RequestTutorAsync(userId, dto);
                return Ok(new { VerificationID = verificationId, message = "Tutor verification request submitted successfully." });
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

        [HttpGet("admin-dashboard-statistics")]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAdminDashboardStatistics()
        {
            try
            {
                // Get all users count
                var allUsers = await _userService.GetAllUsersAsync();
                var totalUsers = allUsers.Count;

                // Get pending verifications count
                var allVerifications = await _tutorVerificationService.GetAllVerificationsAsync();
                var pendingVerifications = allVerifications.Count(v => v.VerificationStatus == "Pending");

                // Get all skills count
                var allSkills = await _skillService.GetAllAsync();
                var totalSkills = allSkills.Count();

                // Get active admins count
                var activeAdmins = allUsers.Count(u => u.Role == "Admin" && u.Status == "Active");

                var statistics = new
                {
                    totalUsers = totalUsers,
                    pendingVerifications = pendingVerifications,
                    totalSkills = totalSkills,
                    activeAdmins = activeAdmins
                };

                return Ok(statistics);
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