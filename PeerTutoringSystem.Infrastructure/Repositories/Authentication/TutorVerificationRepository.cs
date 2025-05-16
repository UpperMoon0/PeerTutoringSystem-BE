using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Authentication
{
    public class TutorVerificationRepository : ITutorVerificationRepository
    {
        private readonly AppDbContext _context;

        public TutorVerificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TutorVerification verification)
        {
            await _context.TutorVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();
        }

        public async Task<TutorVerification> GetByIdAsync(Guid verificationId)
        {
            return await _context.TutorVerifications
                .Include(v => v.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(v => v.VerificationID == verificationId);
        }

        public async Task<IEnumerable<TutorVerification>> GetAllAsync()
        {
            return await _context.TutorVerifications
                .Include(v => v.User)
                .ThenInclude(u => u.Role)
                .ToListAsync();
        }

        public async Task<IEnumerable<TutorVerification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.TutorVerifications
                .Include(v => v.User)
                .ThenInclude(u => u.Role)
                .Where(v => v.UserID == userId)
                .ToListAsync();
        }

        public async Task UpdateAsync(TutorVerification verification)
        {
            _context.TutorVerifications.Update(verification);
            await _context.SaveChangesAsync();
        }
    }
}