using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Skills;
using System;
using System.Security.Claims;

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
            _skillService = skillService;
            _userSkillService = userSkillService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateSkillDto skillDto)
        {
            try
            {
                var skill = await _skillService.AddAsync(skillDto);
                return Ok(new { SkillID = skill.SkillID });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
        }

        [HttpPost("user-skills")]
        public async Task<IActionResult> AddUserSkill([FromBody] UserSkillDto userSkillDto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            if (userId != userSkillDto.UserID && !User.IsInRole("Admin"))
                return Forbid();
            try
            {
                var added = await _userSkillService.AddAsync(userSkillDto);
                return Ok(new { UserSkillID = added.UserSkillID });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user-skills/{userId:guid}")]
        public async Task<IActionResult> GetUserSkills(Guid userId)
        {
            var skills = await _userSkillService.GetByUserIdAsync(userId);
            return Ok(skills.Select(us => new
            {
                UserSkillID = us.UserSkillID,
                SkillID = us.SkillID,
                IsTutor = us.IsTutor
            }));
        }

        [HttpDelete("user-skills/{userSkillId:guid}")]
        public async Task<IActionResult> DeleteUserSkill(Guid userSkillId)
        {
            var userSkill = await _userSkillService.GetByIdAsync(userSkillId);
            if (userSkill == null) return NotFound();
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            if (userId != userSkill.UserID && !User.IsInRole("Admin"))
                return Forbid();
            var success = await _userSkillService.DeleteAsync(userSkillId);
            if (!success) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}