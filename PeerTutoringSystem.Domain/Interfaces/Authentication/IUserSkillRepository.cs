using PeerTutoringSystem.Domain.Entities.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Authentication
{
    public interface IUserSkillRepository
    {
        Task<UserSkill> AddAsync(UserSkill userSkill);
        Task<IEnumerable<UserSkill>> GetAllAsync();
        Task<UserSkill> GetByIdAsync(Guid userSkillId);
        Task<UserSkill> DeleteAsync(Guid userSkillId);
    }
}
