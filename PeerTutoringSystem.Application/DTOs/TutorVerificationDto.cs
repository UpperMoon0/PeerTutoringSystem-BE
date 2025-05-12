using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs
{
    public class RequestTutorDto
    {
        [Required(ErrorMessage = "Citizen ID is required.")]
        public string CitizenID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student ID is required.")]
        public string StudentID { get; set; } = string.Empty;

        [Required(ErrorMessage = "University is required.")]
        public string University { get; set; } = string.Empty;

        [Required(ErrorMessage = "Major is required.")]
        public string Major { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one document is required.")]
        [MinLength(1, ErrorMessage = "At least one document is required.")]
        public List<DocumentUploadDto> Documents { get; set; } = new();
    }

    public class DocumentUploadDto
    {
        [Required(ErrorMessage = "Document type is required.")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document path is required.")]
        public string DocumentPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "File size is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "File size must be greater than 0.")]
        public int FileSize { get; set; }
    }

    public class TutorVerificationDto
    {
        public Guid VerificationID { get; set; }
        public Guid UserID { get; set; }
        public string CitizenID { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string AdminNotes { get; set; } = string.Empty;
        public List<DocumentDto> Documents { get; set; } = new();
    }

    public class DocumentDto
    {
        public Guid DocumentID { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public int FileSize { get; set; }
    }

    public class UpdateTutorVerificationDto
    {
        [Required(ErrorMessage = "Verification status is required.")]
        [RegularExpression("Pending|Approved|Rejected", ErrorMessage = "Verification status must be 'Pending', 'Approved', or 'Rejected'.")]
        public string VerificationStatus { get; set; } = string.Empty;

        public string AdminNotes { get; set; } = string.Empty; // Optional
    }
}