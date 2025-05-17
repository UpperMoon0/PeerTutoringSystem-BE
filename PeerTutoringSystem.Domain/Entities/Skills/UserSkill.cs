using PeerTutoringSystem.Domain.Entities.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Entities.Skills
{
    public class UserSkill
    {
        public Guid UserSkillID { get; set; }
        public Guid UserID { get; set; }
        public Guid SkillID { get; set; }
        public bool IsTutor { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Skill Skill { get; set; }
    }
}
