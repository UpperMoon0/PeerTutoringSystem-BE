using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Entities.Skills;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Authentication
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IUserSkillRepository _userSkillRepository;

        public SkillService(ISkillRepository skillRepository, IUserSkillRepository userSkillRepository)
        {
            _skillRepository = skillRepository;
            _userSkillRepository = userSkillRepository;
        }

        public async Task<SkillDto> AddAsync(CreateSkillDto skillDto)
        {
            if (string.IsNullOrWhiteSpace(skillDto.SkillName))
            {
                throw new InvalidOperationException("SkillName is required.");
            }

            string skillLevelStr = ValidateSkillLevel(skillDto.SkillLevel);

            var existingSkill = await _skillRepository.GetByNameAsync(skillDto.SkillName);
            if (existingSkill != null)
            {
                throw new InvalidOperationException($"Skill with name '{skillDto.SkillName}' already exists.");
            }

            var skill = new Skill
            {
                SkillID = Guid.NewGuid(),
                SkillName = skillDto.SkillName,
                SkillLevel = skillLevelStr,
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

            string skillLevelStr = ValidateSkillLevel(skillDto.SkillLevel);

            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null) return null;

            var existingSkill = await _skillRepository.GetByNameAsync(skillDto.SkillName);
            if (existingSkill != null && existingSkill.SkillID != skillId)
            {
                throw new InvalidOperationException($"Skill with name '{skillDto.SkillName}' already exists.");
            }

            skill.SkillName = skillDto.SkillName;
            skill.SkillLevel = skillLevelStr;
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

        public async Task<bool> DeleteAsync(Guid skillId)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null)
            {
                return false;
            }

            // Check if the skill is associated with any users
            var userSkills = await _userSkillRepository.GetBySkillIdAsync(skillId);
            if (userSkills.Any())
            {
                throw new InvalidOperationException("Cannot delete skill because it is associated with one or more users.");
            }

            await _skillRepository.DeleteAsync(skillId);
            return true;
        }

        private string ValidateSkillLevel(string skillLevelStr)
        {
            if (string.IsNullOrEmpty(skillLevelStr))
                return null;

            if (Enum.TryParse<SkillLevel>(skillLevelStr, true, out _))
                return skillLevelStr;

            throw new InvalidOperationException($"Invalid SkillLevel value: '{skillLevelStr}'. Expected values: Beginner, Elementary, Intermediate, Advanced, or Expert.");
        }
    }
}