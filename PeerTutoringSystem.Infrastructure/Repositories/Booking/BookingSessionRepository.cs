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

        public async Task<(IEnumerable<BookingSession> Bookings, int TotalCount)> GetByStudentIdAsync(Guid studentId, BookingFilter filter)
        {
            var query = _context.BookingSessions.Where(b => b.StudentId == studentId);
            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            return (bookings, totalCount);
        }

        public async Task<IEnumerable<BookingSession>> GetByTutorIdAsync(Guid tutorId)
        {
            return await _context.BookingSessions
                .Where(b => b.TutorId == tutorId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<(IEnumerable<BookingSession> Bookings, int TotalCount)> GetByTutorIdAsync(Guid tutorId, BookingFilter filter)
        {
            var query = _context.BookingSessions.Where(b => b.TutorId == tutorId);
            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            return (bookings, totalCount);
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
            var query = _context.BookingSessions.AsQueryable(); // Fixed the typo "radionu"

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

        public async Task<(IEnumerable<BookingSession> Bookings, int TotalCount)> GetUpcomingBookingsByUserAsync(Guid userId, bool isTutor, BookingFilter filter)
        {
            var query = isTutor
                ? _context.BookingSessions.Where(b => b.TutorId == userId)
                : _context.BookingSessions.Where(b => b.StudentId == userId);

            query = query.Where(b => b.StartTime > DateTime.UtcNow && b.Status != BookingStatus.Cancelled);
            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderBy(b => b.StartTime)
                .ToListAsync();

            return (bookings, totalCount);
        }

        private IQueryable<BookingSession> ApplyFilters(IQueryable<BookingSession> query, BookingFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (Enum.TryParse<BookingStatus>(filter.Status, true, out var status))
                {
                    query = query.Where(b => b.Status == status);
                }
            }

            if (filter.SkillId.HasValue)
            {
                query = query.Where(b => b.SkillId == filter.SkillId.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(b => b.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(b => b.EndTime <= filter.EndDate.Value);
            }

            return query;
        }
    }
}