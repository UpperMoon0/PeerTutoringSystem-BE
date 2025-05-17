using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Authentication
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _skillRepository;

        public SkillService(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<SkillDto> AddAsync(SkillDto skillDto)
        {
            if (string.IsNullOrWhiteSpace(skillDto.SkillName))
            {
                throw new InvalidOperationException("SkillName is required.");
            }

            var existingSkill = await _skillRepository.GetByNameAsync(skillDto.SkillName);
            if (existingSkill != null)
            {
                throw new InvalidOperationException($"Skill with name '{skillDto.SkillName}' already exists.");
            }

            var skill = new Skill
            {
                SkillName = skillDto.SkillName,
                SkillLevel = skillDto.SkillLevel,
                Description = skillDto.Description
            };
            var added = await _skillRepository.AddAsync(skill);
            return new SkillDto
            {
                SkillID = added.SkillID,
                SkillName = added.SkillName,
                SkillLevel = added.SkillLevel,
                Description = added.Description
            };
        }

        public async Task<SkillDto> GetByIdAsync(Guid skillId)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null) return null;
            return new SkillDto
            {
                SkillID = skill.SkillID,
                SkillName = skill.SkillName,
                SkillLevel = skill.SkillLevel,
                Description = skill.Description
            };
        }

        public async Task<IEnumerable<SkillDto>> GetAllAsync()
        {
            var skills = await _skillRepository.GetAllAsync();
            return skills.Select(s => new SkillDto
            {
                SkillID = s.SkillID,
                SkillName = s.SkillName,
                SkillLevel = s.SkillLevel,
                Description = s.Description
            });
        }

        public async Task<SkillDto> UpdateAsync(Guid skillId, SkillDto skillDto)
        {
            if (string.IsNullOrWhiteSpace(skillDto.SkillName))
            {
                throw new InvalidOperationException("SkillName is required.");
            }

            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null) return null;

            var existingSkill = await _skillRepository.GetByNameAsync(skillDto.SkillName);
            if (existingSkill != null && existingSkill.SkillID != skillId)
            {
                throw new InvalidOperationException($"Skill with name '{skillDto.SkillName}' already exists.");
            }

            skill.SkillName = skillDto.SkillName;
            skill.SkillLevel = skillDto.SkillLevel;
            skill.Description = skillDto.Description;
            var updated = await _skillRepository.UpdateAsync(skill);
            return new SkillDto
            {
                SkillID = updated.SkillID,
                SkillName = updated.SkillName,
                SkillLevel = updated.SkillLevel,
                Description = updated.Description
            };
        }
    }
}
