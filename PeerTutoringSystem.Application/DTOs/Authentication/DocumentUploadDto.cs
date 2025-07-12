using System;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class DocumentUploadDto
    {
        [Required(ErrorMessage = "Document type is required.")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document path is required.")]
        public string DocumentPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "File size is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "File size must be greater than 0.")]
        public int FileSize { get; set; }
        public Guid UserID { get; set; }
    }
}