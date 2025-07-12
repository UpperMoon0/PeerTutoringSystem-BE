using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Api.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new { error = "File is required." });
            }

            try
            {
                var result = await _documentService.UploadDocumentAsync(file);
                return Ok(result);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(string id)
        {
            try
            {
                var document = await _documentService.GetDocumentAsync(id);
                if (document == null)
                {
                    return NotFound();
                }

                return File(document.Content, document.ContentType, document.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}