using PeerTutoringSystem.Domain.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Booking
{
    public interface ISessionRepository
    {
        Task<Session> GetByIdAsync(Guid sessionId);
        Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, bool isTutor);
        Task<Session> AddAsync(Session session);
        Task UpdateAsync(Session session);
        Task<(IEnumerable<Session> Sessions, int TotalCount)> GetByUserIdAsync(Guid userId, bool isTutor, BookingFilter filter);
    }
}