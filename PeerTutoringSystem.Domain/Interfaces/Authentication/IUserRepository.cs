using PeerTutoringSystem.Domain.Entities.Authentication;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user); 
    Task<User> GetByFirebaseUidAsync(string firebaseUid);
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task<IEnumerable<User>> GetUsersByIdsAsync(List<Guid> ids);
}