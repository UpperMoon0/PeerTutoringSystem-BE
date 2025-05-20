// PeerTutoringSystem.Infrastructure/Repositories/Booking/BookingSessionRepository.cs
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
    public class BookingSessionRepository : IBookingSessionRepository
    {
        private readonly AppDbContext _context;

        public BookingSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BookingSession booking)
        {
            await _context.BookingSessions.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<BookingSession> GetByIdAsync(Guid bookingId)
        {
            return await _context.BookingSessions
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<IEnumerable<BookingSession>> GetByStudentIdAsync(Guid studentId)
        {
            return await _context.BookingSessions
                .Where(b => b.StudentId == studentId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingSession>> GetByTutorIdAsync(Guid tutorId)
        {
            return await _context.BookingSessions
                .Where(b => b.TutorId == tutorId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(BookingSession booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.BookingSessions.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BookingSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BookingSessions
                .Where(b => b.StartTime >= startDate && b.EndTime <= endDate)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsSlotAvailableAsync(Guid tutorId, DateTime startTime, DateTime endTime)
        {
            // Check if tutor has any overlapping bookings (exclude cancelled)
            var overlappingBookings = await _context.BookingSessions
                .Where(b => b.TutorId == tutorId &&
                            b.Status != BookingStatus.Cancelled &&
                            ((b.StartTime <= startTime && b.EndTime > startTime) ||
                             (b.StartTime < endTime && b.EndTime >= endTime) ||
                             (b.StartTime >= startTime && b.EndTime <= endTime)))
                .AnyAsync();

            return !overlappingBookings;
        }

        public async Task<IEnumerable<BookingSession>> GetUpcomingBookingsByUserAsync(Guid userId, bool isTutor)
        {
            var query = _context.BookingSessions.AsQueryable();

            if (isTutor)
            {
                query = query.Where(b => b.TutorId == userId);
            }
            else
            {
                query = query.Where(b => b.StudentId == userId);
            }

            return await query
                .Where(b => b.StartTime > DateTime.UtcNow && b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }
    }
}
