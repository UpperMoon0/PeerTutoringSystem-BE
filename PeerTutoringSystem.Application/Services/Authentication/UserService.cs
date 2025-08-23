using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Entities.Authentication;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace PeerTutoringSystem.Application.Services.Authentication
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly FirebaseStorageService _firebaseStorageService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserService> _logger;


        public UserService(IUserRepository userRepository, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _firebaseStorageService = new FirebaseStorageService(_configuration, _webHostEnvironment);
            _logger = logger;
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
                School = user.School,
                AvatarUrl = user.AvatarUrl,
                Status = user.Status.ToString(),
                Role = user.Role.RoleName
            };
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
                    School = user.School,
                    AvatarUrl = user.AvatarUrl,
                    Status = user.Status.ToString(),
                    Role = user.Role.RoleName
                });
            }

            return userDtos;
        }

        public async Task<List<UserDto>> GetAllTutorsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var tutorDtos = new List<UserDto>();

            foreach (var user in users)
            {
                if (user.Role.RoleName == "Tutor" && user.Status == UserStatus.Active)
                {
                    tutorDtos.Add(new UserDto
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        Email = user.Email ?? user.FirebaseUid,
                        DateOfBirth = user.DateOfBirth,
                        PhoneNumber = user.PhoneNumber,
                        Gender = user.Gender.ToString(),
                        Hometown = user.Hometown,
                        School = user.School,
                        AvatarUrl = user.AvatarUrl,
                        Status = user.Status.ToString(),
                        Role = user.Role.RoleName
                    });
                }
            }

            return tutorDtos;
        }

        public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            _logger.LogInformation("Attempting to update user with ID: {UserId}", userId);
            ValidateDto(dto);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
            {
                _logger.LogWarning("User with ID: {UserId} not found or inactive.", userId);
                throw new ValidationException("User not found or inactive.");
            }

            if (dto.Email != user.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Email {Email} already exists for another user.", dto.Email);
                    throw new ValidationException("Email already exists.");
                }
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.DateOfBirth = dto.DateOfBirth;
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender = Enum.Parse<Gender>(dto.Gender, true);
            user.Hometown = dto.Hometown;
            user.School = dto.School;
            _logger.LogInformation("User object before avatar update: {User}", user);

            if (dto.Avatar != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(dto.Avatar.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Invalid file format for avatar: {Extension}", extension);
                    throw new ValidationException($"Invalid file format for avatar. Only JPG and PNG files are allowed.");
                }

                var maxFileSize = 2 * 1024 * 1024; // 2MB
                if (dto.Avatar.Length > maxFileSize)
                {
                    _logger.LogWarning("Avatar file size {FileSize} exceeds the maximum size of 2MB.", dto.Avatar.Length);
                    throw new ValidationException($"Avatar exceeds maximum size of 2MB.");
                }

                try
                {
                    var avatarUrl = await _firebaseStorageService.UploadFileAsync(dto.Avatar, "avatars");
                    _logger.LogInformation("Firebase upload successful. Returned URL: {AvatarUrl}", avatarUrl);
                    user.AvatarUrl = avatarUrl;
                    _logger.LogInformation("User object after avatar URL assignment: {User}", user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during Firebase upload for user ID: {UserId}", userId);
                    throw;
                }
            }

            try
            {
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Successfully updated user with ID: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with ID: {UserId} in the database.", userId);
                throw;
            }
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
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnbanUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ValidationException("User not found.");

            if (user.Status != UserStatus.Banned)
                throw new ValidationException("User is not banned and cannot be unbanned.");

            user.Status = UserStatus.Active;
            user.IsOnline = false;
            user.LastActive = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateUserBalanceAsync(Guid userId, UpdateUserBalanceDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ValidationException("User not found.");
            }

            user.AccountBalance = (decimal)dto.AccountBalance;
            await _userRepository.UpdateAsync(user);
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

        public async Task<decimal> GetUserBalance(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ValidationException("User not found.");
            }

            return user.AccountBalance;
        }
    }
}