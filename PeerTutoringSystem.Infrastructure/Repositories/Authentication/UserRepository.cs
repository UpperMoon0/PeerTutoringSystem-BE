using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Infrastructure.Data;

namespace PeerTutoringSystem.Infrastructure.Repositories.Authentication
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddAsync(User user)
        {
            if (user.UserID == Guid.Empty)
            {
                user.UserID = Guid.NewGuid();
            }

            // Đảm bảo RoleID hợp lệ trước khi thêm
            if (user.RoleID != 0) // Kiểm tra RoleID đã được gán
            {
                var role = await _context.Roles.FindAsync(user.RoleID);
                if (role == null)
                {
                    throw new Exception($"Role with ID {user.RoleID} not found.");
                }
                user.Role = role; // Gán đối tượng Role để tránh null
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var existingUser = await _context.Users
                .Include(u => u.Role) // Đảm bảo tải Role khi cập nhật
                .FirstOrDefaultAsync(u => u.UserID == user.UserID);
            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetByFirebaseUidAsync(string firebaseUid)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == roleName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByIdsAsync(List<Guid> ids)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => ids.Contains(u.UserID))
                .ToListAsync();
        }
    }
}