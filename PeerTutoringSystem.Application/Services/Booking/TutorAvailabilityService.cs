using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PeerTutoringSystem.Application.Services.Booking
{
    public class TutorAvailabilityService : ITutorAvailabilityService
    {
        private readonly ITutorAvailabilityRepository _availabilityRepository;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly ILogger<TutorAvailabilityService> _logger;

        public TutorAvailabilityService(
            ITutorAvailabilityRepository availabilityRepository,
            IBookingSessionRepository bookingRepository,
            ILogger<TutorAvailabilityService> logger)
        {
            _availabilityRepository = availabilityRepository
                ?? throw new ArgumentNullException(nameof(availabilityRepository));
            _bookingRepository = bookingRepository
                ?? throw new ArgumentNullException(nameof(bookingRepository));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TutorAvailabilityDto> AddAsync(Guid tutorId, CreateTutorAvailabilityDto dto)
        {
            var currentDateTimeUtc = DateTime.UtcNow;

            if (dto.StartTime < currentDateTimeUtc)
                throw new ValidationException("Start time cannot be in the past.");

            if (dto.EndTime <= dto.StartTime)
                throw new ValidationException("End time must be after start time.");

            if (dto.EndTime.Subtract(dto.StartTime).TotalMinutes < 30)
                throw new ValidationException("Session must be at least 30 minutes long.");

            if (dto.IsRecurring && string.IsNullOrEmpty(dto.RecurringDay) && !dto.IsDailyRecurring)
                throw new ValidationException("Recurring day must be specified for weekly recurring availability.");

            if (dto.RecurrenceEndDate.HasValue && dto.RecurrenceEndDate.Value < currentDateTimeUtc.Date)
                throw new ValidationException("Recurrence end date cannot be in the past.");

            DayOfWeek? recurringDay = null;
            if (dto.IsRecurring && !dto.IsDailyRecurring && !string.IsNullOrEmpty(dto.RecurringDay))
            {
                if (!Enum.TryParse<DayOfWeek>(dto.RecurringDay, true, out var day))
                    throw new ValidationException("Invalid day of week specified.");
                recurringDay = day;
            }

            var availability = new TutorAvailability
            {
                AvailabilityId = Guid.NewGuid(),
                TutorId = tutorId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsRecurring = dto.IsRecurring,
                IsDailyRecurring = dto.IsDailyRecurring,
                RecurringDay = recurringDay,
                RecurrenceEndDate = dto.RecurrenceEndDate,
                IsBooked = false
            };

            await _availabilityRepository.AddAsync(availability);
            return MapToDto(availability);
        }

        public async Task<bool> DeleteAsync(Guid availabilityId)
        {
            try
            {
                var availability = await _availabilityRepository.GetByIdAsync(availabilityId);
                if (availability == null)
                    return false;

                var hasBooking = await _bookingRepository.IsSlotAvailableAsync(
                    availability.TutorId,
                    availability.StartTime,
                    availability.EndTime
                );
                if (!hasBooking)
                    throw new ValidationException("Cannot delete availability with existing bookings.");

                await _availabilityRepository.DeleteAsync(availabilityId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete availability with ID {AvailabilityId}.", availabilityId);
                return false;
            }
        }

        public async Task<(IEnumerable<TutorAvailabilityDto> Availabilities, int TotalCount)> GetByTutorIdAsync(
            Guid tutorId,
            BookingFilterDto filter
        )
        {
            var availabilities = await _availabilityRepository.GetByTutorIdAsync(tutorId);
            var query = availabilities.AsEnumerable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.EndTime <= filter.EndDate.Value);
            }

            var totalCount = query.Count();
            var filtered = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderBy(a => a.StartTime);

            return (filtered.Select(MapToDto), totalCount);
        }

        public async Task<(IEnumerable<TutorAvailabilityDto> Availabilities, int TotalCount)> GetAvailableSlotsAsync(
            Guid tutorId,
            DateTime startDate,
            DateTime endDate,
            BookingFilterDto filter
        )
        {
            if (startDate > endDate)
                throw new ValidationException("Start date must be before end date.");

            var currentDateTimeUtc = DateTime.UtcNow;
            if (startDate < currentDateTimeUtc)
                startDate = currentDateTimeUtc;

            var availabilities = await _availabilityRepository.GetAvailableSlotsByTutorIdAsync(tutorId, startDate, endDate);
            var query = availabilities.AsEnumerable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.EndTime <= filter.EndDate.Value);
            }

            var totalCount = query.Count();
            var filtered = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderBy(a => a.StartTime);

            return (filtered.Select(MapToDto), totalCount);
        }

        public async Task<TutorAvailabilityDto> GetByIdAsync(Guid availabilityId)
        {
            var availability = await _availabilityRepository.GetByIdAsync(availabilityId);
            return availability != null ? MapToDto(availability) : null;
        }

        private TutorAvailabilityDto MapToDto(TutorAvailability availability)
        {
            return availability != null ? new TutorAvailabilityDto
            {
                AvailabilityId = availability.AvailabilityId,
                TutorId = availability.TutorId,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                IsRecurring = availability.IsRecurring,
                IsDailyRecurring = availability.IsDailyRecurring,
                RecurringDay = availability.RecurringDay?.ToString(),
                RecurrenceEndDate = availability.RecurrenceEndDate,
                IsBooked = availability.IsBooked
            } : null;
        }
    }
}