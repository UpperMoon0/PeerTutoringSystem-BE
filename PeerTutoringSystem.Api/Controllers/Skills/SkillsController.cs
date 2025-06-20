using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Skills;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillService;
        private readonly IUserSkillService _userSkillService;

        public SkillsController(ISkillService skillService, IUserSkillService userSkillService)
        {
            _skillService = skillService ?? throw new ArgumentNullException(nameof(skillService));
            _userSkillService = userSkillService ?? throw new ArgumentNullException(nameof(userSkillService));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateSkillDto skillDto)
        {
            if (skillDto == null)
            {
                return BadRequest(new { message = "Request body is required. Please provide a valid skillDto object." });
            }

            try
            {
                var skill = await _skillService.AddAsync(skillDto);
                return Ok(new { SkillID = skill.SkillID });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var skills = await _skillService.GetAllAsync();
            return Ok(skills);
        }

        [HttpGet("{skillId:guid}")]
        public async Task<IActionResult> GetById(Guid skillId)
        {
            var skill = await _skillService.GetByIdAsync(skillId);
            if (skill == null) return NotFound();
            return Ok(skill);
        }

        [HttpPut("{skillId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid skillId, [FromBody] SkillDto skillDto)
        {
            if (skillDto == null)
            {
                return BadRequest(new { message = "Request body is required. Please provide a valid skillDto object." });
            }

            try
            {
                var updated = await _skillService.UpdateAsync(skillId, skillDto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpDelete("{skillId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid skillId)
        {
            try
            {
                var success = await _skillService.DeleteAsync(skillId);
                if (!success) return NotFound(new { message = "Skill not found." });
                return Ok(new { message = "Skill deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("user-skills")]
        public async Task<IActionResult> AddUserSkill([FromBody] UserSkillDto userSkillDto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            if (userId != userSkillDto.UserID && !User.IsInRole("Admin"))
                return StatusCode(403, new { message = "You are not authorized to assegn skills for another user." });

            if (userSkillDto.IsTutor && !User.IsInRole("Tutor") && !User.IsInRole("Admin"))
                return StatusCode(403, new { message = "Only users with Tutor role or Admins can assign a skill as a tutor." });

            try
            {
                var added = await _userSkillService.AddAsync(userSkillDto);
                return Ok(new { UserSkillID = added.UserSkillID });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("user-skills/{userId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserSkills(Guid userId)
        {
            var userSkills = await _userSkillService.GetByUserIdAsync(userId);
            var result = new List<object>();

            foreach (var us in userSkills)
            {
                var skill = await _skillService.GetByIdAsync(us.SkillID);
                result.Add(new
                {
                    UserSkillID = us.UserSkillID,
                    SkillID = us.SkillID,
                    IsTutor = us.IsTutor,
                    Skill = skill != null ? new
                    {
                        SkillID = skill.SkillID,
                        SkillName = skill.SkillName,
                        SkillLevel = skill.SkillLevel,
                        Description = skill.Description
                    } : null
                });
            }

            return Ok(result);
        }

        [HttpDelete("user-skills/{userSkillId:guid}")]
        public async Task<IActionResult> DeleteUserSkill(Guid userSkillId)
        {
            var userSkill = await _userSkillService.GetByIdAsync(userSkillId);
            if (userSkill == null) return NotFound();
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            if (userId != userSkill.UserID && !User.IsInRole("Admin"))
                return StatusCode(403, new { message = "You are not authorized to delete this skill association." });
            var success = await _userSkillService.DeleteAsync(userSkillId);
            if (!success) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}