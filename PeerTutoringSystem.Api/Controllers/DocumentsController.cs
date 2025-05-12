using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces;
using PeerTutoringSystem.Domain.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using PeerTutoringSystem.Domain.Interfaces;

namespace PeerTutoringSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentRepository _documentRepository;
        private readonly ITutorVerificationRepository _tutorVerificationRepository;

        public DocumentsController(
            IDocumentService documentService,
            IDocumentRepository documentRepository,
            ITutorVerificationRepository tutorVerificationRepository)
        {
            _documentService = documentService;
            _documentRepository = documentRepository;
            _tutorVerificationRepository = tutorVerificationRepository;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadDocument([Required] IFormFile file)
        {
            try
            {
                var response = await _documentService.UploadDocumentAsync(file);
                return Ok(response);
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

        [HttpGet("{documentId:guid}")]
        public async Task<IActionResult> GetDocument(Guid documentId)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    return NotFound(new { error = "Document not found." });

                var verification = await _tutorVerificationRepository.GetByIdAsync(document.VerificationID);
                if (verification == null)
                    return NotFound(new { error = "Verification request not found." });

                var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var isAdmin = User.IsInRole("Admin");
                var isTutor = User.IsInRole("Tutor") && verification.UserID == currentUserId;

                if (!isAdmin && !isTutor)
                    return StatusCode(403, new { error = "You do not have permission to access this document." });

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.DocumentPath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { error = "File not found on server." });

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var mimeType = document.DocumentType == "PDF" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return File(fileStream, mimeType, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}