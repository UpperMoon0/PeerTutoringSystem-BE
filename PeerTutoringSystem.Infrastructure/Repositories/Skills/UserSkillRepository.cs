using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Skills;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Skills
{
    public class UserSkillRepository : IUserSkillRepository
    {
        private readonly AppDbContext _context;

        public UserSkillRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSkill> AddAsync(UserSkill userSkill)
        {
            userSkill.UserSkillID = Guid.NewGuid();
            await _context.UserSkills.AddAsync(userSkill);
            await _context.SaveChangesAsync();
            return userSkill;
        }

        public async Task<IEnumerable<UserSkill>> GetAllAsync()
        {
            return await _context.UserSkills
                .Include(us => us.Skill)
                .ToListAsync();
        }

        public async Task<UserSkill> GetByIdAsync(Guid userSkillId)
        {
            return await _context.UserSkills
                .Include(us => us.Skill)
                .FirstOrDefaultAsync(us => us.UserSkillID == userSkillId);
        }

        public async Task<UserSkill> DeleteAsync(Guid userSkillId)
        {
            var userSkill = await _context.UserSkills.FindAsync(userSkillId);
            if (userSkill != null)
            {
                _context.UserSkills.Remove(userSkill);
                await _context.SaveChangesAsync();
            }
            return userSkill;
        }

        public async Task<IEnumerable<UserSkill>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserID == userId)
                .ToListAsync();
        }
    }
}