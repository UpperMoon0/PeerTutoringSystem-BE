// PeerTutoringSystem.Application/Interfaces/Booking/IBookingService.cs
using PeerTutoringSystem.Application.DTOs.Booking;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface IBookingService
    {
        Task<BookingSessionDto> CreateBookingAsync(Guid studentId, CreateBookingDto dto);
        Task<BookingSessionDto> GetBookingByIdAsync(Guid bookingId);
        Task<IEnumerable<BookingSessionDto>> GetBookingsByStudentAsync(Guid studentId);
        Task<IEnumerable<BookingSessionDto>> GetBookingsByTutorAsync(Guid tutorId);
        Task<BookingSessionDto> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusDto dto);
        Task<IEnumerable<BookingSessionDto>> GetUpcomingBookingsAsync(Guid userId, bool isTutor);
    }
}
