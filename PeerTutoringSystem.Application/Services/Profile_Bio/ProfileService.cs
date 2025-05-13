using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces;
using PeerTutoringSystem.Infrastructure.Repositories;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Profile_Bio
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;

        public ProfileService(IProfileRepository profileRepository, IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        public async Task<ProfileDto> CreateProfileAsync(Guid tutorId, CreateProfileDto dto)
        {
            // Validate user
            var user = await _userRepository.GetByIdAsync(tutorId);
            if (user == null || user.Role.RoleName != "Tutor")
                throw new ValidationException("Only tutors can create a profile.");

            // Check if user already has a profile
            var existingProfile = await _profileRepository.GetByUserIdAsync(tutorId);
            if (existingProfile != null)
                throw new ValidationException("User already has a profile.");

            var profile = new Profile
            {
                UserID = tutorId,
                Bio = dto.Bio,
                Experience = dto.Experience,
                HourlyRate = dto.HourlyRate,
                Availability = dto.Availability,
                CreatedDate = DateTime.UtcNow
            };

            await _profileRepository.AddAsync(profile);

            return await MapToDtoAsync(profile);
        }

        public async Task<ProfileDto> GetProfileByIdAsync(int profileId)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId);
            if (profile == null)
                throw new ValidationException("Profile not found.");

            return await MapToDtoAsync(profile);
        }

        public async Task<ProfileDto> GetProfileByUserIdAsync(Guid userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new ValidationException("Profile not found for this user.");

            return await MapToDtoAsync(profile);
        }

        public async Task UpdateProfileAsync(int profileId, UpdateProfileDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId);
            if (profile == null)
                throw new ValidationException("Profile not found.");

            profile.Bio = dto.Bio;
            profile.Experience = dto.Experience;
            profile.HourlyRate = dto.HourlyRate;
            profile.Availability = dto.Availability;
            profile.UpdatedDate = DateTime.UtcNow;

            await _profileRepository.UpdateAsync(profile);
        }

        private async Task<ProfileDto> MapToDtoAsync(Profile profile)
        {
            var user = await _userRepository.GetByIdAsync(profile.UserID);
            if (user == null)
                throw new ValidationException("User not found.");

            return new ProfileDto
            {
                ProfileID = profile.ProfileID,
                UserID = profile.UserID,
                TutorName = user.FullName,
                Bio = profile.Bio,
                Experience = profile.Experience,
                HourlyRate = profile.HourlyRate,
                Availability = profile.Availability,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                School = user.School,
                CreatedDate = profile.CreatedDate,
                UpdatedDate = profile.UpdatedDate
            };
        }
    }
}