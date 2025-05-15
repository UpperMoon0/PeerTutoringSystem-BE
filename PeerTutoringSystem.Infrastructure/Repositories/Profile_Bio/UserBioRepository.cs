using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Profile_Bio
{
    public class UserBioRepository : IUserBioRepository 
    {
        private readonly AppDbContext _context;

        public UserBioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserBio> GetByIdAsync(int bioId) 
        {
            return await _context.UserBio 
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.BioID == bioId); 
        }

        public async Task<UserBio> GetByUserIdAsync(Guid userId) 
        {
            return await _context.UserBio 
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserID == userId);
        }

        public async Task AddAsync(UserBio userBio) 
        {
            await _context.UserBio.AddAsync(userBio); 
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserBio userBio)
        {
            _context.UserBio.Update(userBio); 
            await _context.SaveChangesAsync();
        }
    }
}