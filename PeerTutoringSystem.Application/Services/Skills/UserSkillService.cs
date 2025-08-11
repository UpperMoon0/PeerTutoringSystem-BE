using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.Interfaces.Skills;
using PeerTutoringSystem.Domain.Entities.Skills;
using PeerTutoringSystem.Domain.Interfaces.Skills;

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

        public async Task<UserSkillDto> AddAsync(CreateUserSkillDto userSkillDto)
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
                UserSkillID = Guid.NewGuid(),
                UserID = userSkillDto.UserID,
                SkillID = userSkillDto.SkillID,
                IsTutor = userSkillDto.IsTutor
            };
            var added = await _userSkillRepository.AddAsync(userSkill);

            return new UserSkillDto
            {
                UserSkillID = added.UserSkillID,
                UserID = added.UserID,
                IsTutor = added.IsTutor,
                Skill = new SkillDto
                {
                    SkillID = added.SkillID,
                    SkillName = skill.SkillName,
                    Description = skill.Description,
                    SkillLevel = skill.SkillLevel
                }
            };
        }

        public async Task<IEnumerable<UserSkillDto>> GetByUserIdAsync(Guid userId)
        {
            var userSkills = await _userSkillRepository.GetByUserIdAsync(userId);
            var userSkillDtos = new List<UserSkillDto>();
            foreach (var us in userSkills)
            {
                var skill = await _skillRepository.GetByIdAsync(us.Skill.SkillID);
                userSkillDtos.Add(new UserSkillDto
                {
                    UserSkillID = us.UserSkillID,
                    UserID = us.UserID,
                    IsTutor = us.IsTutor,
                    Skill = skill != null ? new SkillDto
                    {
                        SkillID = skill.SkillID,
                        SkillName = skill.SkillName,
                        Description = skill.Description,
                        SkillLevel = skill.SkillLevel
                    } : null
                });
            }
            return userSkillDtos;
        }

        public async Task<IEnumerable<UserSkillDto>> GetAllAsync()
        {
            var userSkills = await _userSkillRepository.GetAllAsync();
            var userSkillDtos = new List<UserSkillDto>();
            foreach (var us in userSkills)
            {
                var skill = await _skillRepository.GetByIdAsync(us.Skill.SkillID);
                userSkillDtos.Add(new UserSkillDto
                {
                    UserSkillID = us.UserSkillID,
                    UserID = us.UserID,
                    IsTutor = us.IsTutor,
                    Skill = skill != null ? new SkillDto
                    {
                        SkillID = skill.SkillID,
                        SkillName = skill.SkillName,
                        Description = skill.Description,
                        SkillLevel = skill.SkillLevel
                    } : null
                });
            }
            return userSkillDtos;
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
                IsTutor = userSkill.IsTutor,
                Skill = userSkill.Skill != null ? new SkillDto
                {
                    SkillID = userSkill.Skill.SkillID,
                    SkillName = userSkill.Skill.SkillName,
                    Description = userSkill.Skill.Description,
                    SkillLevel = userSkill.Skill.SkillLevel
                } : null
            };
        }
    }
}