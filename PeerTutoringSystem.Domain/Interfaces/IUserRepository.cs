using PeerTutoringSystem.Domain.Entities;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user); // Add this method for updates
    Task<User> GetByFirebaseUidAsync(string firebaseUid);
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
}