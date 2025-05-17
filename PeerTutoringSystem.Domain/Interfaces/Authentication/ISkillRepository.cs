using PeerTutoringSystem.Domain.Entities.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Authentication
{
    public interface ISkillRepository
    {
        Task<Skill> AddAsync(Skill skill);
        Task<Skill> GetByIdAsync(Guid skillId);
        Task<IEnumerable<Skill>> GetAllAsync();
        Task<Skill> UpdateAsync(Skill skill);
    }
}
