using PeerTutoringSystem.Domain.Entities.Skills;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Skills
{
    public interface ISkillRepository
    {
        Task<Skill> AddAsync(Skill skill);
        Task<Skill> GetByIdAsync(Guid skillId);
        Task<Skill> GetByNameAsync(string skillName);
        Task<IEnumerable<Skill>> GetAllAsync();
        Task<Skill> UpdateAsync(Skill skill);
        Task DeleteAsync(Guid skillId);
    }
}