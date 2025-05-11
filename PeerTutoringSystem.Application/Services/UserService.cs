using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces;
using PeerTutoringSystem.Domain.Entities;
using PeerTutoringSystem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
                throw new ValidationException("User not found or inactive.");

            return new UserDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender.ToString(),
                Hometown = user.Hometown,
                AvatarUrl = user.AvatarUrl,
                Status = user.Status.ToString(),
                Role = user.Role.RoleName
            };
        }

        public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            ValidateDto(dto);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
                throw new ValidationException("User not found or inactive.");

            if (dto.Email != user.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                    throw new ValidationException("Email already exists.");
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.DateOfBirth = dto.DateOfBirth;
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender = Enum.Parse<Gender>(dto.Gender, true);
            user.Hometown = dto.Hometown;
            user.AvatarUrl = dto.AvatarUrl;
            await _userRepository.UpdateAsync(user); // Sửa từ AddAsync thành UpdateAsync
        }

        public async Task BanUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ValidationException("User not found.");

            if (user.Status == UserStatus.Banned)
                throw new ValidationException("User is already banned.");

            user.Status = UserStatus.Banned;
            user.IsOnline = false;
            user.LastActive = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user); // Sửa từ AddAsync thành UpdateAsync
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                userDtos.Add(new UserDto
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    Email = user.Email ?? user.FirebaseUid,
                    DateOfBirth = user.DateOfBirth,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender.ToString(),
                    Hometown = user.Hometown,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status.ToString(),
                    Role = user.Role.RoleName
                });
            }

            return userDtos;
        }

        private void ValidateDto<T>(T dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}