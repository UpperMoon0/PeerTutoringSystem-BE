using PeerTutoringSystem.Application.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Authentication
{
    public interface IUserSkillService
    {
        Task<UserSkillDto> AddAsync(UserSkillDto userSkillDto);
        Task<IEnumerable<UserSkillDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserSkillDto>> GetAllAsync();
        Task<bool> DeleteAsync(Guid userSkillId);
        Task<UserSkillDto> GetByIdAsync(Guid userSkillId);
    }
}