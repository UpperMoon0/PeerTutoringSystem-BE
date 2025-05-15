using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Entities;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IUserTokenRepository userTokenRepository,
        IConfiguration configuration,
        AppDbContext context,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        ValidateDto(dto);

        if (!IsOver14(dto.DateOfBirth))
            throw new ValidationException("You must be over 14 years old to register.");

        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new ValidationException("Email is already registered.");

            var user = new User
            {
                UserID = Guid.NewGuid(),
                Email = dto.Email,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Gender = ParseGender(dto.Gender),
                Hometown = dto.Hometown,
                AvatarUrl = string.Empty,
                CreatedDate = DateTime.UtcNow,
                LastActive = DateTime.UtcNow,
                IsOnline = true,
                Status = UserStatus.Active,
                RoleID = 1, // Student
                FirebaseUid = null // Đặt null cho đăng ký email/mật khẩu
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            var createdUser = await _userRepository.AddAsync(user);

            var userWithRole = await _userRepository.GetByIdAsync(createdUser.UserID);
            if (userWithRole == null || userWithRole.Role == null)
                throw new Exception("Failed to load user role after registration.");

            var (accessToken, refreshToken) = GenerateJwtToken(userWithRole);
            await StoreTokenAsync(userWithRole.UserID, accessToken, refreshToken);

            await transaction.CommitAsync();

            _logger.LogInformation("Registered user with email: {Email}, FirebaseUid: {FirebaseUid}", dto.Email, user.FirebaseUid);

            return new AuthResponseDto
            {
                UserID = userWithRole.UserID,
                FullName = userWithRole.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AvatarUrl = userWithRole.AvatarUrl,
                Role = userWithRole.Role.RoleName
            };
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique constraint") == true)
        {
            _logger.LogError(ex, "Unique constraint violation during registration for email: {Email}", dto.Email);
            await transaction.RollbackAsync();
            throw new ValidationException("Email is already registered (race condition detected).", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency exception during registration for email: {Email}", dto.Email);
            await transaction.RollbackAsync();
            throw new ValidationException("A concurrency issue occurred. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}", dto.Email);
            await transaction.RollbackAsync();
            throw new Exception("An unexpected error occurred: " + ex.Message, ex);
        }
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
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
        if (string.IsNullOrEmpty(firebaseUid))
        {
            throw new ValidationException("FirebaseUid cannot be empty.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            var user = await _userRepository.GetByFirebaseUidAsync(firebaseUid);

            if (user != null)
            {
                if (user.Status == UserStatus.Banned)
                    throw new ValidationException("Your account has been banned.");

                user.LastActive = DateTime.UtcNow;
                user.IsOnline = true;
                await _userRepository.UpdateAsync(user);

                var (accessToken, refreshToken) = GenerateJwtToken(user);
                await StoreTokenAsync(user.UserID, accessToken, refreshToken);

                await transaction.CommitAsync();

                return new AuthResponseDto
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AvatarUrl = user.AvatarUrl,
                    Role = user.Role.RoleName
                };
            }

            if (!IsOver14(dto.DateOfBirth))
                throw new ValidationException("You must be over 14 years old to register.");

            user = new User
            {
                UserID = Guid.NewGuid(),
                FirebaseUid = firebaseUid,
                Email = decodedToken.Claims["email"]?.ToString() ?? string.Empty,
                FullName = dto.FullName,
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

            var userWithRole = await _userRepository.GetByIdAsync(createdUser.UserID);
            if (userWithRole == null || userWithRole.Role == null)
                throw new Exception("Failed to load user role after Google login.");

            var (newAccessToken, newRefreshToken) = GenerateJwtToken(userWithRole);
            await StoreTokenAsync(userWithRole.UserID, newAccessToken, newRefreshToken);

            await transaction.CommitAsync();

            return new AuthResponseDto
            {
                UserID = userWithRole.UserID,
                FullName = userWithRole.FullName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AvatarUrl = userWithRole.AvatarUrl,
                Role = userWithRole.Role.RoleName
            };
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique constraint") == true)
        {
            _logger.LogError(ex, "Unique constraint violation during Google login for FirebaseUid: {FirebaseUid}", firebaseUid);
            await transaction.RollbackAsync();

            var existingUser = await _userRepository.GetByFirebaseUidAsync(firebaseUid);
            if (existingUser != null)
            {
                if (existingUser.Status == UserStatus.Banned)
                    throw new ValidationException("Your account has been banned.");

                existingUser.LastActive = DateTime.UtcNow;
                existingUser.IsOnline = true;
                await _userRepository.UpdateAsync(existingUser);

                var (accessToken, refreshToken) = GenerateJwtToken(existingUser);
                await StoreTokenAsync(existingUser.UserID, accessToken, refreshToken);

                return new AuthResponseDto
                {
                    UserID = existingUser.UserID,
                    FullName = existingUser.FullName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AvatarUrl = existingUser.AvatarUrl,
                    Role = existingUser.Role.RoleName
                };
            }

            throw new ValidationException("Failed to register user with FirebaseUid (race condition detected).", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency exception during Google login for FirebaseUid: {FirebaseUid}", firebaseUid);
            await transaction.RollbackAsync();
            throw new ValidationException("A concurrency issue occurred. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Google login for FirebaseUid: {FirebaseUid}", firebaseUid);
            await transaction.RollbackAsync();
            throw new Exception("An unexpected error occurred: " + ex.Message, ex);
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        ValidateDto(dto);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) != PasswordVerificationResult.Success)
                throw new ValidationException("Invalid email or password.");

            if (user.Status == UserStatus.Banned)
                throw new ValidationException("Your account has been banned.");

            user.LastActive = DateTime.UtcNow;
            user.IsOnline = true;
            await _userRepository.UpdateAsync(user);

            var (accessToken, refreshToken) = GenerateJwtToken(user);
            await StoreTokenAsync(user.UserID, accessToken, refreshToken);

            await transaction.CommitAsync();

            return new AuthResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.RoleName
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency exception during login for email: {Email}", dto.Email);
            await transaction.RollbackAsync();
            throw new ValidationException("A concurrency issue occurred. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", dto.Email);
            await transaction.RollbackAsync();
            throw new Exception("An unexpected error occurred: " + ex.Message, ex);
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        ValidateDto(dto);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var token = await _userTokenRepository.GetByRefreshTokenAsync(dto.RefreshToken);
            if (token == null || token.IsRevoked || token.RefreshTokenExpiresAt < DateTime.UtcNow)
                throw new ValidationException("Invalid or expired refresh token.");

            var user = await _userRepository.GetByIdAsync(token.UserID);
            if (user == null || user.Status != UserStatus.Active)
                throw new ValidationException("User not found or inactive.");

            var (accessToken, refreshToken) = GenerateJwtToken(user);
            await StoreTokenAsync(user.UserID, accessToken, refreshToken);

            token.IsRevoked = true;
            await _userTokenRepository.UpdateAsync(token);

            await transaction.CommitAsync();

            return new AuthResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.RoleName
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency exception during token refresh");
            await transaction.RollbackAsync();
            throw new ValidationException("A concurrency issue occurred. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            await transaction.RollbackAsync();
            throw new Exception("An unexpected error occurred: " + ex.Message, ex);
        }
    }

    public async Task LogoutAsync(Guid userId, string accessToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ValidationException("User not found.");

            user.IsOnline = false;
            user.LastActive = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = await _userTokenRepository.GetByAccessTokenAsync(accessToken);
            if (token != null && !token.IsRevoked)
            {
                token.IsRevoked = true;
                await _userTokenRepository.UpdateAsync(token);
            }

            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency exception during logout for userId: {UserId}", userId);
            await transaction.RollbackAsync();
            throw new ValidationException("A concurrency issue occurred. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout for userId: {UserId}", userId);
            await transaction.RollbackAsync();
            throw new Exception("An unexpected error occurred: " + ex.Message, ex);
        }
    }

    private (string accessToken, string refreshToken) GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? user.FirebaseUid ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.RoleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60"));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: accessTokenExpiry,
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
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7")),
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