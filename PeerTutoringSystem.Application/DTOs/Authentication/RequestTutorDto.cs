using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs.Authentication
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
}