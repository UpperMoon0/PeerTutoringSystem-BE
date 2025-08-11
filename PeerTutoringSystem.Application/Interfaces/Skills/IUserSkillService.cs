using PeerTutoringSystem.Application.DTOs.Skills;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Skills
{
    public interface IUserSkillService
    {
        Task<UserSkillDto> AddAsync(CreateUserSkillDto userSkillDto);
        Task<IEnumerable<UserSkillDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserSkillDto>> GetAllAsync();
        Task<bool> DeleteAsync(Guid userSkillId);
        Task<UserSkillDto> GetByIdAsync(Guid userSkillId);
    }
}