using System;

namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public enum SkillLevel
    {
        Beginner,
        Elementary,
        Intermediate,
        Advanced,
        Expert
    }

    public class CreateSkillDto
    {
        public string SkillName { get; set; }
        public string SkillLevel { get; set; } 
        public string Description { get; set; }
    }

    public class SkillDto
    {
        public Guid SkillID { get; set; }
        public string SkillName { get; set; }
        public string SkillLevel { get; set; } 
        public string Description { get; set; }
    }
}