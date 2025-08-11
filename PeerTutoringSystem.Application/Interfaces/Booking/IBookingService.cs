using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Domain.Entities.Booking;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface IBookingService
    {
        Task<BookingSessionDto> CreateBookingAsync(Guid studentId, CreateBookingDto dto);
        Task<BookingSessionDto> CreateInstantBookingAsync(Guid studentId, InstantBookingDto dto);
        Task<BookingSessionDto> GetBookingByIdAsync(Guid bookingId);
        Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetBookingsByStudentAsync(Guid studentId, BookingFilterDto filter);
        Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetBookingsByTutorAsync(Guid tutorId, BookingFilterDto filter);
        Task<BookingSessionDto> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusDto dto);
        Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetUpcomingBookingsAsync(Guid userId, bool isTutor, BookingFilterDto filter);
        Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetAllBookingsForAdminAsync(BookingFilterDto filter);
        Task<TutorDashboardStatsDto> GetTutorDashboardStatsAsync(Guid tutorId);
        Task<BookingSessionDto> AcceptBookingAsync(Guid bookingId, Guid tutorId);
        Task<BookingSessionDto> RejectBookingAsync(Guid bookingId, Guid tutorId);
        Task<(bool Succeeded, string Message)> ConfirmPayment(Guid bookingId, PaymentConfirmationDto paymentConfirmationDto);
        Task<(bool Succeeded, string Message)> UploadProofOfPayment(Guid bookingId, IFormFile file);
    }
}