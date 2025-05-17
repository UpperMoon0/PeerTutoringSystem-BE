using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Entities.Authentication
{
    public class Skill
    {
        public Guid SkillID { get; set; }
        public string SkillName { get; set; }
        public string SkillLevel { get; set; }
        public string Description { get; set; }
    }
}
