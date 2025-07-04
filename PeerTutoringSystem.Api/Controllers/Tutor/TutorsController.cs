using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Tutor;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Tutor
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TutorsController : ControllerBase
    {
        private readonly ITutorService _tutorService;

        public TutorsController(ITutorService tutorService)
        {
            _tutorService = tutorService;
        }

        [HttpGet("enriched-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEnrichedTutors()
        {
            var result = await _tutorService.GetAllEnrichedTutorsAsync();
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return BadRequest(new { error = result.Error });
        }
        
        [HttpGet("enriched/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEnrichedTutorById(string id)
        {
            var result = await _tutorService.GetEnrichedTutorByIdAsync(id);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return NotFound(new { error = result.Error });
        }
    }
}