using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class UserSkillDto
    {
        public Guid? UserSkillID { get; set; } // Chỉ có trong response
        public Guid UserID { get; set; } // Yêu cầu trong request
        public Guid SkillID { get; set; }
        public bool IsTutor { get; set; }
        public SkillDto Skill { get; set; } // Chỉ có trong response của GET
    }
}
