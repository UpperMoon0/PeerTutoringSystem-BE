using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Infrastructure.Data;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Authentication
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly AppDbContext _context;

        public UserTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserToken> AddAsync(UserToken token)
        {
            await _context.UserTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<UserToken> UpdateAsync(UserToken token)
        {
            var existingToken = await _context.UserTokens.FirstOrDefaultAsync(t => t.TokenID == token.TokenID);
            if (existingToken == null)
                throw new Exception("Token not found.");

            _context.Entry(existingToken).CurrentValues.SetValues(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<UserToken> GetByAccessTokenAsync(string accessToken)
        {
            return await _context.UserTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.AccessToken == accessToken);
        }

        public async Task<UserToken> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.UserTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
        }
    }
}