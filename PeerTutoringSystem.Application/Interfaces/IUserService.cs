using PeerTutoringSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task UpdateUserAsync(Guid userId, UpdateUserDto dto);
        Task BanUserAsync(Guid userId);
        Task<List<UserDto>> GetAllUsersAsync();
    }
}