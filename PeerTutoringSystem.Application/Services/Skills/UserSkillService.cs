using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.Interfaces.Skills;
using PeerTutoringSystem.Domain.Entities.Skills;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Authentication
{
    public class UserSkillService : IUserSkillService
    {
        private readonly IUserSkillRepository _userSkillRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IUserRepository _userRepository;

        public UserSkillService(IUserSkillRepository userSkillRepository, ISkillRepository skillRepository, IUserRepository userRepository)
        {
            _userSkillRepository = userSkillRepository;
            _skillRepository = skillRepository;
            _userRepository = userRepository;
        }

        public async Task<UserSkillDto> AddAsync(UserSkillDto userSkillDto)
        {
            var user = await _userRepository.GetByIdAsync(userSkillDto.UserID);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userSkillDto.UserID}' does not exist.");
            }

            var skill = await _skillRepository.GetByIdAsync(userSkillDto.SkillID);
            if (skill == null)
            {
                throw new InvalidOperationException($"Skill with ID '{userSkillDto.SkillID}' does not exist.");
            }

            var userSkill = new UserSkill
            {
                UserID = userSkillDto.UserID,
                SkillID = userSkillDto.SkillID,
                IsTutor = userSkillDto.IsTutor
            };
            var added = await _userSkillRepository.AddAsync(userSkill);
            return new UserSkillDto
            {
                UserSkillID = added.UserSkillID,
                UserID = added.UserID,
                SkillID = added.SkillID,
                IsTutor = added.IsTutor,
                Skill = new SkillDto
                {
                    SkillID = skill.SkillID,
                    SkillName = skill.SkillName,
                    SkillLevel = skill.SkillLevel, // Đã là string
                    Description = skill.Description
                }
            };
        }

        public async Task<IEnumerable<UserSkillDto>> GetByUserIdAsync(Guid userId)
        {
            var userSkills = await _userSkillRepository.GetByUserIdAsync(userId);
            return userSkills.Select(us => new UserSkillDto
            {
                UserSkillID = us.UserSkillID,
                UserID = us.UserID,
                SkillID = us.SkillID,
                IsTutor = us.IsTutor,
                Skill = us.Skill != null ? new SkillDto
                {
                    SkillID = us.Skill.SkillID,
                    SkillName = us.Skill.SkillName,
                    SkillLevel = us.Skill.SkillLevel, // Đã là string
                    Description = us.Skill.Description
                } : null
            });
        }

        public async Task<IEnumerable<UserSkillDto>> GetAllAsync()
        {
            var userSkills = await _userSkillRepository.GetAllAsync();
            return userSkills.Select(us => new UserSkillDto
            {
                UserSkillID = us.UserSkillID,
                UserID = us.UserID,
                SkillID = us.SkillID,
                IsTutor = us.IsTutor,
                Skill = us.Skill != null ? new SkillDto
                {
                    SkillID = us.Skill.SkillID,
                    SkillName = us.Skill.SkillName,
                    SkillLevel = us.Skill.SkillLevel, // Đã là string
                    Description = us.Skill.Description
                } : null
            });
        }

        public async Task<bool> DeleteAsync(Guid userSkillId)
        {
            var userSkill = await _userSkillRepository.GetByIdAsync(userSkillId);
            if (userSkill == null) return false;
            await _userSkillRepository.DeleteAsync(userSkillId);
            return true;
        }

        public async Task<UserSkillDto> GetByIdAsync(Guid userSkillId)
        {
            var userSkill = await _userSkillRepository.GetByIdAsync(userSkillId);
            if (userSkill == null) return null;
            return new UserSkillDto
            {
                UserSkillID = userSkill.UserSkillID,
                UserID = userSkill.UserID,
                SkillID = userSkill.SkillID,
                IsTutor = userSkill.IsTutor,
                Skill = userSkill.Skill != null ? new SkillDto
                {
                    SkillID = userSkill.Skill.SkillID,
                    SkillName = userSkill.Skill.SkillName,
                    SkillLevel = userSkill.Skill.SkillLevel, // Đã là string
                    Description = userSkill.Skill.Description
                } : null
            };
        }
    }
}