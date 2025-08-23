using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Api.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TutorVerificationsController : ControllerBase
    {
        private readonly ITutorVerificationService _tutorVerificationService;
        private readonly IDocumentService _documentService;
        private readonly IDocumentRepository _documentRepository;

        public TutorVerificationsController(ITutorVerificationService tutorVerificationService, IDocumentService documentService, IDocumentRepository documentRepository)
        {
            _tutorVerificationService = tutorVerificationService;
            _documentService = documentService;
            _documentRepository = documentRepository;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestTutor([FromBody] RequestTutorDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                var verificationId = await _tutorVerificationService.RequestTutorAsync(userId, dto);
                return Ok(new { verificationId });
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
        public async Task<IActionResult> GetVerificationsByUserId(Guid userId)
        {
            try
            {
                var verifications = await _tutorVerificationService.GetVerificationsByUserIdAsync(userId);
                return Ok(verifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllVerifications()
        {
            try
            {
                var verifications = await _tutorVerificationService.GetAllVerificationsAsync();
                return Ok(verifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("{verificationId:guid}")]
        public async Task<IActionResult> GetVerificationById(Guid verificationId)
        {
            try
            {
                var verification = await _tutorVerificationService.GetVerificationByIdAsync(verificationId);
                return Ok(verification);
            }
            catch (ValidationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpPut("{verificationId:guid}")]
        [Authorize(Roles = "Admin")]
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

        [HttpGet("pending/{userId:guid}")]
        public async Task<IActionResult> CheckPendingTutorVerification(Guid userId)
        {
            try
            {
                var result = await _tutorVerificationService.HasPendingOrApprovedTutorVerificationAsync(userId, "Pending", "Approved");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
        [HttpGet("document/{documentId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadDocument(Guid documentId)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    return NotFound(new { error = "Document not found." });
                }

                if (string.IsNullOrEmpty(document.DocumentPath))
                {
                    return NotFound(new { error = "Document path not found." });
                }
                var documentDto = await _documentService.GetDocumentAsync(document.DocumentPath);
                return File(documentDto.Content, documentDto.ContentType, documentDto.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}