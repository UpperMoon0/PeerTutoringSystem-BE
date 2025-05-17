using PeerTutoringSystem.Application.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Authentication
{
    public interface ISkillService
    {
        Task<SkillDto> AddAsync(Guid skillId, SkillDto skillDto);
        Task<SkillDto> GetByIdAsync(Guid skillId);
        Task<IEnumerable<SkillDto>> GetAllAsync();
        Task<SkillDto> UpdateAsync(Guid skillId, SkillDto skillDto);
    }
}