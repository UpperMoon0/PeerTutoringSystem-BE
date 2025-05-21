using PeerTutoringSystem.Domain.Entities.Skills;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Skills
{
    public interface IUserSkillRepository
    {
        Task<UserSkill> AddAsync(UserSkill userSkill);
        Task<IEnumerable<UserSkill>> GetAllAsync();
        Task<UserSkill> GetByIdAsync(Guid userSkillId);
        Task<UserSkill> DeleteAsync(Guid userSkillId);
        Task<IEnumerable<UserSkill>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserSkill>> GetBySkillIdAsync(Guid skillId);
    }
}