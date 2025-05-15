using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Profile_Bio
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserBio> GetByIdAsync(int profileId)
        {
            return await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProfileID == profileId);
        }

        public async Task<UserBio> GetByUserIdAsync(Guid userId)
        {
            return await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserID == userId);
        }

        public async Task AddAsync(UserBio profile)
        {
            await _context.Profiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserBio profile)
        {
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}