using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Infrastructure.Repositories;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Profile_Bio
{
    public class UserBioService : IUserBioService // Đổi tên class và interface
    {
        private readonly IUserBioRepository _profileRepository;
        private readonly IUserRepository _userRepository;

        public UserBioService(IUserBioRepository profileRepository, IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        public async Task<UserBioDto> CreateProfileAsync(Guid tutorId, CreateUserBioDto dto)
        {
            // Validate user
            var user = await _userRepository.GetByIdAsync(tutorId);
            if (user == null || user.Role.RoleName != "Tutor")
                throw new ValidationException("Only tutors can create a user bio.");

            // Check if user already has a user bio
            var existingUserBio = await _profileRepository.GetByUserIdAsync(tutorId);
            if (existingUserBio != null)
                throw new ValidationException("User already has a user bio.");

            var userBio = new UserBio 
            {
                UserID = tutorId,
                Bio = dto.Bio,
                Experience = dto.Experience,
                HourlyRate = dto.HourlyRate,
                Availability = dto.Availability,
                CreatedDate = DateTime.UtcNow
            };

            await _profileRepository.AddAsync(userBio);

            return await MapToDtoAsync(userBio);
        }

        public async Task<UserBioDto> GetProfileByIdAsync(int bioId) 
        {
            var userBio = await _profileRepository.GetByIdAsync(bioId); 
            if (userBio == null)
                throw new ValidationException("User bio not found.");

            return await MapToDtoAsync(userBio);
        }

        public async Task<UserBioDto> GetProfileByUserIdAsync(Guid userId) 
        {
            var userBio = await _profileRepository.GetByUserIdAsync(userId); 
            if (userBio == null)
                throw new ValidationException("User bio not found for this user.");

            return await MapToDtoAsync(userBio);
        }

        public async Task UpdateProfileAsync(int bioId, UpdateUserBioDto dto) 
        {
            var userBio = await _profileRepository.GetByIdAsync(bioId); 
            if (userBio == null)
                throw new ValidationException("User bio not found.");

            userBio.Bio = dto.Bio;
            userBio.Experience = dto.Experience;
            userBio.HourlyRate = dto.HourlyRate;
            userBio.Availability = dto.Availability;
            userBio.UpdatedDate = DateTime.UtcNow;

            await _profileRepository.UpdateAsync(userBio);
        }

        private async Task<UserBioDto> MapToDtoAsync(UserBio userBio)
        {
            var user = await _userRepository.GetByIdAsync(userBio.UserID);
            if (user == null)
                throw new ValidationException("User not found.");

            return new UserBioDto
            {
                BioID = userBio.BioID, 
                UserID = userBio.UserID,
                TutorName = user.FullName,
                Bio = userBio.Bio,
                Experience = userBio.Experience,
                HourlyRate = userBio.HourlyRate,
                Availability = userBio.Availability,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                School = user.School,
                CreatedDate = userBio.CreatedDate,
                UpdatedDate = userBio.UpdatedDate
            };
        }
    }
}