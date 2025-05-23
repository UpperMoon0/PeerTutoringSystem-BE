using PeerTutoringSystem.Application.DTOs.Booking;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface IBookingService
    {
        Task<BookingSessionDto> CreateBookingAsync(Guid studentId, CreateBookingDto dto);
        Task<BookingSessionDto> GetBookingByIdAsync(Guid bookingId);
        Task<PagedResultDto<BookingSessionDto>> GetBookingsByStudentAsync(Guid studentId, BookingFilterDto filter);
        Task<PagedResultDto<BookingSessionDto>> GetBookingsByTutorAsync(Guid tutorId, BookingFilterDto filter);
        Task<BookingSessionDto> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusDto dto);
        Task<PagedResultDto<BookingSessionDto>> GetUpcomingBookingsAsync(Guid userId, bool isTutor, BookingFilterDto filter);
    }
}