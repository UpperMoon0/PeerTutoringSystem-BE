using PeerTutoringSystem.Application.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}