using PeerTutoringSystem.Domain.Entities.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Authentication
{
    public interface ITutorVerificationRepository
    {
        Task AddAsync(TutorVerification verification);
        Task<TutorVerification> GetByIdAsync(Guid verificationId);
        Task<IEnumerable<TutorVerification>> GetAllAsync();
        Task UpdateAsync(TutorVerification verification);
        Task<IEnumerable<TutorVerification>> GetByUserIdAsync(Guid userId);
    }
}