using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities;
using PeerTutoringSystem.Domain.Interfaces;
using PeerTutoringSystem.Infrastructure.Data;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly AppDbContext _context;

        public UserTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserToken token)
        {
            await _context.UserTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<UserToken> GetByAccessTokenAsync(string accessToken)
        {
            return await _context.UserTokens
                .FirstOrDefaultAsync(t => t.AccessToken == accessToken);
        }

        public async Task<UserToken> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.UserTokens
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
        }

        public async Task UpdateAsync(UserToken token)
        {
            _context.UserTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}