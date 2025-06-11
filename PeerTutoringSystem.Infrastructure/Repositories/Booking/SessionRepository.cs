using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Booking
{
    public class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _context;

        public SessionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Session> AddAsync(Session session)
        {
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<Session> GetByIdAsync(Guid sessionId)
        {
            return await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<Session> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Sessions
                .FirstOrDefaultAsync(s => s.BookingId == bookingId);
        }

        public async Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, bool isTutor)
        {
            var query = _context.Sessions
                .Join(_context.BookingSessions,
                    s => s.BookingId,
                    b => b.BookingId,
                    (s, b) => new { Session = s, Booking = b });

            if (isTutor)
            {
                query = query.Where(x => x.Booking.TutorId == userId);
            }
            else
            {
                query = query.Where(x => x.Booking.StudentId == userId);
            }

            var sessions = await query
                .Select(x => x.Session)
                .ToListAsync();

            return sessions;
        }

        public async Task<(IEnumerable<Session> Sessions, int TotalCount)> GetByUserIdAsync(Guid userId, bool isTutor, BookingFilter filter)
        {
            var query = _context.Sessions
                .Join(_context.BookingSessions,
                    s => s.BookingId,
                    b => b.BookingId,
                    (s, b) => new { Session = s, Booking = b });

            if (isTutor)
            {
                query = query.Where(x => x.Booking.TutorId == userId);
            }
            else
            {
                query = query.Where(x => x.Booking.StudentId == userId);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.Session.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(x => x.Session.EndTime <= filter.EndDate.Value);
            }

            var totalCount = await query.CountAsync();
            var sessions = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => x.Session)
                .ToListAsync();

            return (sessions, totalCount);
        }

        public async Task UpdateAsync(Session session)
        {
            session.UpdatedAt = DateTime.UtcNow;
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
        }
    }
}