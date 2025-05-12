using PeerTutoringSystem.Domain.Entities.Authentication;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Authentication
{
    public interface IUserTokenRepository
    {
        Task AddAsync(UserToken token);
        Task<UserToken> GetByAccessTokenAsync(string accessToken);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateAsync(UserToken token);
    }
}