using System;

namespace PeerTutoringSystem.Application.DTOs.Skills
{
    public class CreateUserSkillDto
    {
        public Guid UserID { get; set; }
        public Guid SkillID { get; set; }
        public bool IsTutor { get; set; }
    }
}