using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface ISessionService
    {
        Task<SessionDto> CreateSessionAsync(Guid userId, CreateSessionDto dto);
        Task<SessionDto> GetSessionByIdAsync(Guid sessionId);
        Task<(IEnumerable<SessionDto> Sessions, int TotalCount)> GetSessionsByUserAsync(Guid userId, bool isTutor, BookingFilterDto filter);
        Task<SessionDto> UpdateSessionAsync(Guid sessionId, UpdateSessionDto dto);
    }
}