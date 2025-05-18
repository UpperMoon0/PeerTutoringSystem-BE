// PeerTutoringSystem.Domain/Interfaces/Booking/IBookingSessionRepository.cs
using PeerTutoringSystem.Domain.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Booking
{
    public interface IBookingSessionRepository
    {
        Task<BookingSession> GetByIdAsync(Guid bookingId);
        Task<IEnumerable<BookingSession>> GetByStudentIdAsync(Guid studentId);
        Task<IEnumerable<BookingSession>> GetByTutorIdAsync(Guid tutorId);
        Task AddAsync(BookingSession booking);
        Task UpdateAsync(BookingSession booking);
        Task<IEnumerable<BookingSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> IsSlotAvailableAsync(Guid tutorId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<BookingSession>> GetUpcomingBookingsByUserAsync(Guid userId, bool isTutor);
    }
}