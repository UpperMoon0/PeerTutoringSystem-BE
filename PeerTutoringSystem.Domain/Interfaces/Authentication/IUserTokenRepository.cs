using PeerTutoringSystem.Domain.Entities.Authentication;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Authentication
{
    public interface IUserTokenRepository
    {
        Task<UserToken> AddAsync(UserToken token);
        Task<UserToken> UpdateAsync(UserToken token);
        Task<UserToken> GetByAccessTokenAsync(string accessToken);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken);
    }
}