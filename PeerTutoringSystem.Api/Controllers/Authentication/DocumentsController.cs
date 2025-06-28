using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;

namespace PeerTutoringSystem.Api.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentRepository _documentRepository;
        private readonly ITutorVerificationRepository _tutorVerificationRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DocumentsController(
            IDocumentService documentService,
            IDocumentRepository documentRepository,
            ITutorVerificationRepository tutorVerificationRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _documentService = documentService;
            _documentRepository = documentRepository;
            _tutorVerificationRepository = tutorVerificationRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }
                var currentUserId = Guid.Parse(userIdString);
                var isAdmin = User.IsInRole("Admin");
                var isTutor = User.IsInRole("Tutor") && verification.UserID == currentUserId;

                if (!isAdmin && !isTutor)
                    return StatusCode(403, new { error = "You do not have permission to access this document." });

                var firebaseStorageUrl = document.DocumentPath;
                if (string.IsNullOrEmpty(firebaseStorageUrl))
                {
                    return NotFound(new { error = "Document URL not found." });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(firebaseStorageUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound(new { error = "File not found in Firebase Storage or access denied." });
                }

                var fileStream = await response.Content.ReadAsStreamAsync();
                var mimeType = document.DocumentType == "PDF" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                var fileName = System.IO.Path.GetFileName(new Uri(firebaseStorageUrl).LocalPath); // Extract filename from URL
                return File(fileStream, mimeType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}