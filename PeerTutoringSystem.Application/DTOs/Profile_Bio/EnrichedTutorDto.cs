using PeerTutoringSystem.Application.DTOs.Skills;
using System;
using System.Collections.Generic;

namespace PeerTutoringSystem.Application.DTOs.Profile_Bio
{
    public class EnrichedTutorDto
    {
        public Guid UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public string Availability { get; set; } = string.Empty;
        public string? School { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public IEnumerable<UserSkillDto> Skills { get; set; } = new List<UserSkillDto>();
    }
}