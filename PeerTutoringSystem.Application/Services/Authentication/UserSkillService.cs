using Microsoft.EntityFrameworkCore;
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

            var userSkill = new UserSkill
            {
                UserID = userSkillDto.UserID,
                SkillID = userSkillDto.SkillID,
                IsTutor = userSkillDto.IsTutor
            };
            var added = await _userSkillRepository.AddAsync(userSkill);
            var skill = await _skillRepository.GetByIdAsync(added.SkillID);
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
                    SkillLevel = skill.SkillLevel,
                    Description = skill.Description
                }
            };
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
                Skill = new SkillDto
                {
                    SkillID = us.Skill.SkillID,
                    SkillName = us.Skill.SkillName,
                    SkillLevel = us.Skill.SkillLevel,
                    Description = us.Skill.Description
                }
            });
        }

        public async Task<bool> DeleteAsync(Guid userSkillId)
        {
            var userSkill = await _userSkillRepository.GetByIdAsync(userSkillId);
            if (userSkill == null) return false;
            await _userSkillRepository.DeleteAsync(userSkillId);
            return true;
        }
    }
}
