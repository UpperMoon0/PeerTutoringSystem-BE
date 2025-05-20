// PeerTutoringSystem.Application/Services/Booking/BookingService.cs
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly ITutorAvailabilityRepository _availabilityRepository;
        private readonly IUserService _userService;

        public BookingService(
            IBookingSessionRepository bookingRepository,
            ITutorAvailabilityRepository availabilityRepository,
            IUserService userService)
        {
            _bookingRepository = bookingRepository;
            _availabilityRepository = availabilityRepository;
            _userService = userService;
        }

        public async Task<BookingSessionDto> CreateBookingAsync(Guid studentId, CreateBookingDto dto)
        {
            // Get availability
            var availability = await _availabilityRepository.GetByIdAsync(dto.AvailabilityId);
            if (availability == null)
                throw new ValidationException("The selected time slot is not available.");

            // Validate tutor ID matches availability
            if (availability.TutorId != dto.TutorId)
                throw new ValidationException("The selected time slot does not belong to the specified tutor.");

            // Check if slot is already booked
            if (availability.IsBooked)
                throw new ValidationException("This time slot has already been booked.");

            // Check if slot is still available (not double-booked)
            if (!await _bookingRepository.IsSlotAvailableAsync(dto.TutorId, availability.StartTime, availability.EndTime))
                throw new ValidationException("This time slot is no longer available.");

            // Create booking
            var booking = new BookingSession
            {
                BookingId = Guid.NewGuid(),
                StudentId = studentId,
                TutorId = dto.TutorId,
                AvailabilityId = dto.AvailabilityId,
                SessionDate = availability.StartTime.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                SkillId = dto.SkillId,
                Topic = dto.Topic ?? "General tutoring session",
                Description = dto.Description,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);

            // Mark availability as booked
            availability.IsBooked = true;
            await _availabilityRepository.UpdateAsync(availability);

            // Return booking with names
            return await EnrichBookingWithNames(booking);
        }

        public async Task<BookingSessionDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return null;

            return await EnrichBookingWithNames(booking);
        }

        public async Task<IEnumerable<BookingSessionDto>> GetBookingsByStudentAsync(Guid studentId)
        {
            var bookings = await _bookingRepository.GetByStudentIdAsync(studentId);
            var dtos = new List<BookingSessionDto>();

            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return dtos;
        }

        public async Task<IEnumerable<BookingSessionDto>> GetBookingsByTutorAsync(Guid tutorId)
        {
            var bookings = await _bookingRepository.GetByTutorIdAsync(tutorId);
            var dtos = new List<BookingSessionDto>();

            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return dtos;
        }

        public async Task<IEnumerable<BookingSessionDto>> GetUpcomingBookingsAsync(Guid userId, bool isTutor)
        {
            var upcomingBookings = await _bookingRepository.GetUpcomingBookingsByUserAsync(userId, isTutor);
            var dtos = new List<BookingSessionDto>();

            foreach (var booking in upcomingBookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return dtos;
        }

        public async Task<BookingSessionDto> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ValidationException("Booking not found.");

            // Validate status
            if (!Enum.TryParse<BookingStatus>(dto.Status, true, out var newStatus))
                throw new ValidationException("Invalid booking status.");

            // Apply status-specific validations
            switch (newStatus)
            {
                case BookingStatus.Cancelled:
                    if (booking.Status == BookingStatus.Completed)
                        throw new ValidationException("Cannot cancel a completed booking.");

                    // If cancelled, make the slot available again
                    var availability = await _availabilityRepository.GetByIdAsync(booking.AvailabilityId);
                    if (availability != null)
                    {
                        availability.IsBooked = false;
                        await _availabilityRepository.UpdateAsync(availability);
                    }
                    break;

                case BookingStatus.Completed:
                    if (booking.Status == BookingStatus.Cancelled)
                        throw new ValidationException("Cannot complete a cancelled booking.");
                    if (booking.EndTime > DateTime.UtcNow)
                        throw new ValidationException("Cannot mark a future booking as completed.");
                    break;

                case BookingStatus.Confirmed:
                    if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                        throw new ValidationException("Cannot confirm a booking that is cancelled or completed.");
                    break;
            }

            // Update booking status
            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);

            return await EnrichBookingWithNames(booking);
        }

        private async Task<BookingSessionDto> EnrichBookingWithNames(BookingSession booking)
        {
            // Get student and tutor names
            var studentName = "Unknown Student";
            var tutorName = "Unknown Tutor";

            try
            {
                var student = await _userService.GetUserByIdAsync(booking.StudentId.ToString());
                if (student != null)
                {
                    studentName = $"{student.FirstName} {student.LastName}";
                }
            }
            catch { /* Ignore errors and use default name */ }

            try
            {
                var tutor = await _userService.GetUserByIdAsync(booking.TutorId.ToString());
                if (tutor != null)
                {
                    tutorName = $"{tutor.FirstName} {tutor.LastName}";
                }
            }
            catch { /* Ignore errors and use default name */ }

            return new BookingSessionDto
            {
                BookingId = booking.BookingId,
                StudentId = booking.StudentId,
                TutorId = booking.TutorId,
                SessionDate = booking.SessionDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                SkillId = booking.SkillId,
                Topic = booking.Topic,
                Description = booking.Description,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt,
                StudentName = studentName,
                TutorName = tutorName
            };
        }
    }
}