using System;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs.Profile_Bio
{
    public class UserBioDto
    {
        public int BioID { get; set; } 
        public Guid UserID { get; set; }
        public string TutorName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public string Availability { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string? School { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateUserBioDto 
    {
        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(0, 1000000, ErrorMessage = "Hourly rate must be between 0 and 1,000,000.")]
        public decimal HourlyRate { get; set; }

        public string Bio { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
    }

    public class UpdateUserBioDto 
    {
        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(0, 1000000, ErrorMessage = "Hourly rate must be between 0 and 1,000,000.")]
        public decimal HourlyRate { get; set; }

        public string Bio { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
    }
}