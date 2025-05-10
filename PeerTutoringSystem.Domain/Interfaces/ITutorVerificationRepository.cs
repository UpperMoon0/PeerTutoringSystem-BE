using PeerTutoringSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces
{
    public interface ITutorVerificationRepository
    {
        Task AddAsync(TutorVerification verification);
        Task<TutorVerification> GetByIdAsync(Guid verificationId);
        Task<IEnumerable<TutorVerification>> GetAllAsync();
        Task UpdateAsync(TutorVerification verification);
    }
}