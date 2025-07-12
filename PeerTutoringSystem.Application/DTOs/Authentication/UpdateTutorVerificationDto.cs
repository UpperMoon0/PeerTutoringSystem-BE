using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class UpdateTutorVerificationDto
    {
        [Required(ErrorMessage = "Verification status is required.")]
        [RegularExpression("Pending|Approved|Rejected", ErrorMessage = "Verification status must be 'Pending', 'Approved', or 'Rejected'.")]
        public string VerificationStatus { get; set; } = string.Empty;

        public string AdminNotes { get; set; } = string.Empty; // Optional
    }
}