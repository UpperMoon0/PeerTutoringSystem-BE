using PeerTutoringSystem.Application.DTOs.Authentication;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task LogoutAsync(Guid userId, string accessToken);
    }
}