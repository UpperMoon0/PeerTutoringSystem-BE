using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Authentication
{
    public class DocumentService : IDocumentService
    {
        private readonly FirebaseStorageService _firebaseStorageService;

        public DocumentService(FirebaseStorageService firebaseStorageService)
        {
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<DocumentResponseDto> UploadDocumentAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new ValidationException($"Invalid file format for {file.FileName}. Only PDF and Word files are allowed.");

            var maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                throw new ValidationException($"File {file.FileName} exceeds maximum size of 5MB.");

            var documentUrl = await _firebaseStorageService.UploadFileAsync(file, "documents");

            return new DocumentResponseDto
            {
                DocumentPath = documentUrl,
                DocumentType = extension == ".pdf" ? "PDF" : "Word",
                FileSize = (int)file.Length
            };
        }

        public async Task<DocumentDto> GetDocumentAsync(string id)
        {
            var (content, contentType, fileName) = await _firebaseStorageService.DownloadFileAsync(id);

            return new DocumentDto
            {
                Content = content,
                ContentType = contentType,
                FileName = fileName
            };
        }
    }
}