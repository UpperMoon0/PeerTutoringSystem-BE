using System;

namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class CreateSkillDto
    {
        public string SkillName { get; set; }
        public string SkillLevel { get; set; }
        public string Description { get; set; }
    }
}