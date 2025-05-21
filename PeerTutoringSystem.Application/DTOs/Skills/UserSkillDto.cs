using PeerTutoringSystem.Application.DTOs.Authentication;
using System;

namespace PeerTutoringSystem.Application.DTOs.Skills
{
    public class UserSkillDto
    {
        public Guid? UserSkillID { get; set; }
        public Guid UserID { get; set; }
        public Guid SkillID { get; set; }
        public bool IsTutor { get; set; }
    }
}