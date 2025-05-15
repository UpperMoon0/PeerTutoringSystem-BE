using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Profile_Bio
{
    public interface IUserBioService 
    {
        Task<UserBioDto> CreateProfileAsync(Guid tutorId, CreateUserBioDto dto); 
        Task<UserBioDto> GetProfileByIdAsync(int bioId); 
        Task<UserBioDto> GetProfileByUserIdAsync(Guid userId); 
        Task UpdateProfileAsync(int bioId, UpdateUserBioDto dto);
    }
}