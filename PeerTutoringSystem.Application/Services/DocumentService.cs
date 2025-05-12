using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly string _documentStoragePath;

        public DocumentService()
        {
            _documentStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
            if (!Directory.Exists(_documentStoragePath))
                Directory.CreateDirectory(_documentStoragePath);
        }

        public async Task<DocumentResponseDto> UploadDocumentAsync(IFormFile file)
        {
            // Kiểm tra định dạng tệp (PDF hoặc Word)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new ValidationException($"Invalid file format for {file.FileName}. Only PDF and Word files are allowed.");

            // Kiểm tra kích thước tệp (tối đa 5MB)
            var maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                throw new ValidationException($"File {file.FileName} exceeds maximum size of 5MB.");

            // Tạo tên tệp duy nhất
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_documentStoragePath, fileName);

            // Lưu tệp vào thư mục
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về thông tin tệp
            return new DocumentResponseDto
            {
                DocumentPath = $"/documents/{fileName}",
                DocumentType = extension == ".pdf" ? "PDF" : "Word",
                FileSize = (int)file.Length
            };
        }
    }
}