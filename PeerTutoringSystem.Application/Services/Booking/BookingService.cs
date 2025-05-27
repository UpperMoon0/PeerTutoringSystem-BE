using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Skills;
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
        private readonly ISkillRepository _skillRepository;

        public BookingService(
            IBookingSessionRepository bookingRepository,
            ITutorAvailabilityRepository availabilityRepository,
            IUserService userService,
            ISkillRepository skillRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _availabilityRepository = availabilityRepository ?? throw new ArgumentNullException(nameof(availabilityRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
        }

        public async Task<BookingSessionDto> CreateBookingAsync(Guid studentId, CreateBookingDto dto)
        {
            var availability = await _availabilityRepository.GetByIdAsync(dto.AvailabilityId);
            if (availability == null)
                throw new ValidationException("The selected time slot is not available.");

            if (availability.TutorId != dto.TutorId)
                throw new ValidationException("The selected time slot does not belong to the specified tutor.");

            if (availability.IsBooked)
                throw new ValidationException("This time slot has already been booked.");

            if (!await _bookingRepository.IsSlotAvailableAsync(dto.TutorId, availability.StartTime, availability.EndTime))
                throw new ValidationException("This time slot is no longer available.");

            if (dto.SkillId.HasValue)
            {
                var skill = await _skillRepository.GetByIdAsync(dto.SkillId.Value);
                if (skill == null)
                    throw new ValidationException("The specified skill does not exist.");
            }

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

            availability.IsBooked = true;
            await _availabilityRepository.UpdateAsync(availability);

            return await EnrichBookingWithNames(booking);
        }

        public async Task<BookingSessionDto> CreateInstantBookingAsync(Guid studentId, InstantBookingDto dto)
        {
            var currentDateTimeUtc = DateTime.UtcNow;

            if (dto.StartTime < currentDateTimeUtc)
                throw new ValidationException("Start time cannot be in the past.");
            if (dto.EndTime <= dto.StartTime)
                throw new ValidationException("End time must be after start time.");
            if (dto.EndTime.Subtract(dto.StartTime).TotalMinutes < 30)
                throw new ValidationException("Session must be at least 30 minutes long.");

            if (!await _bookingRepository.IsSlotAvailableAsync(dto.TutorId, dto.StartTime, dto.EndTime))
                throw new ValidationException("This time slot is not available.");

            if (dto.SkillId.HasValue)
            {
                var skill = await _skillRepository.GetByIdAsync(dto.SkillId.Value);
                if (skill == null)
                    throw new ValidationException("The specified skill does not exist.");
            }

            var availability = new TutorAvailability
            {
                AvailabilityId = Guid.NewGuid(),
                TutorId = dto.TutorId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsRecurring = false,
                IsDailyRecurring = false,
                IsBooked = true
            };
            await _availabilityRepository.AddAsync(availability);

            var booking = new BookingSession
            {
                BookingId = Guid.NewGuid(),
                StudentId = studentId,
                TutorId = dto.TutorId,
                AvailabilityId = availability.AvailabilityId,
                SessionDate = dto.StartTime.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SkillId = dto.SkillId,
                Topic = dto.Topic ?? "General tutoring session",
                Description = dto.Description,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);
            return await EnrichBookingWithNames(booking);
        }

        public async Task<BookingSessionDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return null;

            return await EnrichBookingWithNames(booking);
        }

        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetBookingsByStudentAsync(Guid studentId, BookingFilterDto filter)
        {
            var query = await _bookingRepository.GetByStudentIdAsync(studentId);
            query = ApplyFilters(query, filter);

            var totalCount = query.Count();
            var bookings = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetBookingsByTutorAsync(Guid tutorId, BookingFilterDto filter)
        {
            var query = await _bookingRepository.GetByTutorIdAsync(tutorId);
            query = ApplyFilters(query, filter);

            var totalCount = query.Count();
            var bookings = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetUpcomingBookingsAsync(Guid userId, bool isTutor, BookingFilterDto filter)
        {
            var query = await _bookingRepository.GetUpcomingBookingsByUserAsync(userId, isTutor);
            query = ApplyFilters(query, filter);

            var totalCount = query.Count();
            var bookings = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<BookingSessionDto> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ValidationException("Booking not found.");

            if (!Enum.TryParse<BookingStatus>(dto.Status, true, out var newStatus))
                throw new ValidationException("Invalid booking status.");

            // Kiểm tra trạng thái chuyển đổi hợp lệ
            if (!IsValidStatusTransition(booking.Status, newStatus))
                throw new ValidationException($"Invalid status transition from {booking.Status} to {newStatus}.");

            switch (newStatus)
            {
                case BookingStatus.Cancelled:
                    var availability = await _availabilityRepository.GetByIdAsync(booking.AvailabilityId);
                    if (availability != null)
                    {
                        availability.IsBooked = false;
                        await _availabilityRepository.UpdateAsync(availability);
                    }
                    break;

                case BookingStatus.Completed:
                    if (booking.EndTime > DateTime.UtcNow)
                        throw new ValidationException("Cannot mark a future booking as completed.");
                    break;
            }

            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);

            return await EnrichBookingWithNames(booking);
        }

        private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
        {
            switch (currentStatus)
            {
                case BookingStatus.Pending:
                    return newStatus == BookingStatus.Confirmed || newStatus == BookingStatus.Cancelled;
                case BookingStatus.Confirmed:
                    return newStatus == BookingStatus.Completed || newStatus == BookingStatus.Cancelled;
                case BookingStatus.Cancelled:
                case BookingStatus.Completed:
                    return false; // Không cho phép chuyển đổi từ Cancelled hoặc Completed
                default:
                    return false;
            }
        }

        private IEnumerable<BookingSession> ApplyFilters(IEnumerable<BookingSession> query, BookingFilterDto filter)
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

            return query.OrderByDescending(b => b.StartTime);
        }

        private async Task<BookingSessionDto> EnrichBookingWithNames(BookingSession booking)
        {
            var studentName = "Unknown Student";
            var tutorName = "Unknown Tutor";

            try
            {
                var student = await _userService.GetUserByIdAsync(booking.StudentId);
                if (student != null)
                {
                    studentName = $"{student.FullName}";
                }
            }
            catch { }

            try
            {
                var tutor = await _userService.GetUserByIdAsync(booking.TutorId);
                if (tutor != null)
                {
                    tutorName = $"{tutor.FullName}";
                }
            }
            catch { }

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