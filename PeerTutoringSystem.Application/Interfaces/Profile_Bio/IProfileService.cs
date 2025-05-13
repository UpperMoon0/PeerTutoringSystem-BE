using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Profile_Bio
{
    public interface IProfileService
    {
        Task<ProfileDto> CreateProfileAsync(Guid tutorId, CreateProfileDto dto);
        Task<ProfileDto> GetProfileByIdAsync(int profileId);
        Task<ProfileDto> GetProfileByUserIdAsync(Guid userId);
        Task UpdateProfileAsync(int profileId, UpdateProfileDto dto);
    }
}