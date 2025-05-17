using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;

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
        public async Task<IActionResult> Add([FromBody] SkillDto skillDto)
        {
            var skill = await _skillService.AddAsync(skillDto);
            return Ok(skill);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var skills = await _skillService.GetAllAsync();
            return Ok(skills);
        }

        [HttpGet("{skillId}")]
        public async Task<IActionResult> GetById(Guid skillId)
        {
            var skill = await _skillService.GetByIdAsync(skillId);
            if (skill == null) return NotFound();
            return Ok(skill);
        }

        [HttpPut("{skillId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid skillId, [FromBody] SkillDto skillDto)
        {
            var updated = await _skillService.UpdateAsync(skillId, skillDto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPost("user-skills")]
        public async Task<IActionResult> AddUserSkill([FromBody] UserSkillDto userSkillDto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            userSkillDto.UserID = userId;
            var added = await _userSkillService.AddAsync(userSkillDto);
            return Ok(added);
        }

        [HttpGet("user-skills")]
        public async Task<IActionResult> GetUserSkills()
        {
            var skills = await _userSkillService.GetAllAsync();
            return Ok(skills);
        }

        [HttpDelete("user-skills/{userSkillId}")]
        public async Task<IActionResult> DeleteUserSkill(Guid userSkillId)
        {
            var success = await _userSkillService.DeleteAsync(userSkillId);
            if (!success) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
