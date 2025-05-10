using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces;
using PeerTutoringSystem.Domain.Entities;
using PeerTutoringSystem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(IUserRepository userRepository, IUserTokenRepository userTokenRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userTokenRepository = userTokenRepository;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Validate DTO
            ValidateDto(dto);

            // Check age requirement
            if (!IsOver14(dto.DateOfBirth))
                throw new ValidationException("You must be over 14 years old to register.");

            // Check for existing email
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new ValidationException("Email is already registered.");

            var user = new User
            {
                UserID = Guid.NewGuid(),
                Email = dto.Email,
                AnonymousName = dto.AnonymousName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Gender = ParseGender(dto.Gender),
                Hometown = dto.Hometown,
                AvatarUrl = string.Empty,
                CreatedDate = DateTime.UtcNow,
                LastActive = DateTime.UtcNow,
                IsOnline = true,
                Status = UserStatus.Active,
                RoleID = 1 // Student
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            var createdUser = await _userRepository.AddAsync(user);

            var (accessToken, refreshToken) = GenerateJwtToken(createdUser);
            await StoreTokenAsync(createdUser.UserID, accessToken, refreshToken);

            return new AuthResponseDto
            {
                UserID = createdUser.UserID,
                AnonymousName = createdUser.AnonymousName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AvatarUrl = createdUser.AvatarUrl,
                Role = createdUser.Role.RoleName
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Validate DTO
            ValidateDto(dto);

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) != PasswordVerificationResult.Success)
                throw new ValidationException("Invalid email or password.");

            if (user.Status == UserStatus.Banned)
                throw new ValidationException("Your account has been banned.");

            user.LastActive = DateTime.UtcNow;
            user.IsOnline = true;
            await _userRepository.AddAsync(user);

            var (accessToken, refreshToken) = GenerateJwtToken(user);
            await StoreTokenAsync(user.UserID, accessToken, refreshToken);

            return new AuthResponseDto
            {
                UserID = user.UserID,
                AnonymousName = user.AnonymousName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.RoleName
            };
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
        {
            // Validate DTO
            ValidateDto(dto);

            FirebaseToken decodedToken;
            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(dto.IdToken);
            }
            catch
            {
                throw new ValidationException("Invalid Google ID Token.");
            }

            string firebaseUid = decodedToken.Uid;
            var user = await _userRepository.GetByFirebaseUidAsync(firebaseUid);

            if (user != null)
            {
                if (user.Status == UserStatus.Banned)
                    throw new ValidationException("Your account has been banned.");

                user.LastActive = DateTime.UtcNow;
                user.IsOnline = true;
                await _userRepository.AddAsync(user);

                var (accessToken, refreshToken) = GenerateJwtToken(user);
                await StoreTokenAsync(user.UserID, accessToken, refreshToken);

                return new AuthResponseDto
                {
                    UserID = user.UserID,
                    AnonymousName = user.AnonymousName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AvatarUrl = user.AvatarUrl,
                    Role = user.Role.RoleName
                };
            }

            // Check age requirement
            if (!IsOver14(dto.DateOfBirth))
                throw new ValidationException("You must be over 14 years old to register.");

            user = new User
            {
                UserID = Guid.NewGuid(),
                FirebaseUid = firebaseUid,
                Email = decodedToken.Claims["email"]?.ToString() ?? string.Empty,
                AnonymousName = dto.AnonymousName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Gender = ParseGender(dto.Gender),
                Hometown = dto.Hometown,
                AvatarUrl = string.Empty,
                CreatedDate = DateTime.UtcNow,
                LastActive = DateTime.UtcNow,
                IsOnline = true,
                Status = UserStatus.Active,
                RoleID = 1 // Student
            };

            var createdUser = await _userRepository.AddAsync(user);
            var (newAccessToken, newRefreshToken) = GenerateJwtToken(createdUser);
            await StoreTokenAsync(createdUser.UserID, newAccessToken, newRefreshToken);

            return new AuthResponseDto
            {
                UserID = createdUser.UserID,
                AnonymousName = createdUser.AnonymousName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AvatarUrl = createdUser.AvatarUrl,
                Role = createdUser.Role.RoleName
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // Validate DTO
            ValidateDto(dto);

            var token = await _userTokenRepository.GetByRefreshTokenAsync(dto.RefreshToken);
            if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                throw new ValidationException("Invalid or expired refresh token.");

            var user = await _userRepository.GetByIdAsync(token.UserID);
            if (user == null || user.Status != UserStatus.Active)
                throw new ValidationException("User not found or inactive.");

            var (accessToken, refreshToken) = GenerateJwtToken(user);
            await StoreTokenAsync(user.UserID, accessToken, refreshToken);

            token.IsRevoked = true;
            await _userTokenRepository.UpdateAsync(token);

            return new AuthResponseDto
            {
                UserID = user.UserID,
                AnonymousName = user.AnonymousName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.RoleName
            };
        }

        public async Task LogoutAsync(Guid userId, string accessToken)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ValidationException("User not found.");

            user.IsOnline = false;
            user.LastActive = DateTime.UtcNow;
            await _userRepository.AddAsync(user);

            var token = await _userTokenRepository.GetByAccessTokenAsync(accessToken);
            if (token != null && !token.IsRevoked)
            {
                token.IsRevoked = true;
                await _userTokenRepository.UpdateAsync(token);
            }
        }

        private (string accessToken, string refreshToken) GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? user.FirebaseUid),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Guid.NewGuid().ToString();

            return (accessToken, refreshToken);
        }

        private async Task StoreTokenAsync(Guid userId, string accessToken, string refreshToken)
        {
            var token = new UserToken
            {
                TokenID = Guid.NewGuid(),
                UserID = userId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsRevoked = false
            };

            await _userTokenRepository.AddAsync(token);
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

        private bool IsOver14(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth > today.AddYears(-age)) age--;
            return age >= 14;
        }

        private Gender ParseGender(string gender)
        {
            if (!Enum.TryParse<Gender>(gender, true, out var parsedGender))
                throw new ValidationException("Gender must be 'Male', 'Female', or 'Other'.");
            return parsedGender;
        }
    }
}