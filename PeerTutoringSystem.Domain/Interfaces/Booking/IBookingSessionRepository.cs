using PeerTutoringSystem.Domain.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Booking
{
    public interface IBookingSessionRepository
    {
        Task<BookingSession?> GetByIdAsync(Guid bookingId);
        Task<IEnumerable<BookingSession>> GetByStudentIdAsync(Guid studentId);
        Task<(IEnumerable<BookingSession> Bookings, int TotalCount)> GetByStudentIdAsync(Guid studentId, BookingFilter filter);
        Task<IEnumerable<BookingSession>> GetByTutorIdAsync(Guid tutorId);
        Task<(IEnumerable<BookingSession> Bookings, int TotalCount)> GetByTutorIdAsync(Guid tutorId, BookingFilter filter);
        Task AddAsync(BookingSession booking);
        Task UpdateAsync(BookingSession booking);
        Task<IEnumerable<BookingSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<BookingSession>> GetUpcomingBookingsByUserAsync(Guid userId, bool isTutor);
        Task<(IEnumerable<BookingSession> Bookings, int TotalCount)> GetUpcomingBookingsByUserAsync(Guid userId, bool isTutor, BookingFilter filter);
        Task<IEnumerable<BookingSession>> GetAllAsync();// cho admin lấy all booking
        Task<BookingSession> GetByOrderCode(long orderCode);
        Task<bool> IsSlotAvailableAsync(Guid tutorId, DateTime startTime, DateTime endTime);
    }
}