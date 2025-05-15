using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Profile_Bio
{
    public interface IProfileService
    {
        Task<UserBioDto> CreateProfileAsync(Guid tutorId, CreateProfileDto dto);
        Task<UserBioDto> GetProfileByIdAsync(int profileId);
        Task<UserBioDto> GetProfileByUserIdAsync(Guid userId);
        Task UpdateProfileAsync(int profileId, UpdateProfileDto dto);
    }
}