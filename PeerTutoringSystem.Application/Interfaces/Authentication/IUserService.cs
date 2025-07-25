using PeerTutoringSystem.Application.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Authentication
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task UpdateUserAsync(Guid userId, UpdateUserDto dto);
        Task BanUserAsync(Guid userId);
        Task UnbanUserAsync(Guid userId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<UserDto>> GetAllTutorsAsync();
    }
}