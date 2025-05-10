using PeerTutoringSystem.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces
{
    public interface IUserTokenRepository
    {
        Task AddAsync(UserToken token);
        Task<UserToken> GetByAccessTokenAsync(string accessToken);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateAsync(UserToken token);
    }
}