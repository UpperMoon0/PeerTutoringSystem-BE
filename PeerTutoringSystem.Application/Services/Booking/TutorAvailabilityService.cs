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
        private readonly ILogger<TutorAvailabilityService> _logger;

        public TutorAvailabilityService(ITutorAvailabilityRepository availabilityRepository, ILogger<TutorAvailabilityService> logger)
        {
            _availabilityRepository = availabilityRepository;
            _logger = logger;
        }

        public async Task<TutorAvailabilityDto> AddAsync(Guid tutorId, CreateTutorAvailabilityDto dto)
        {
            var currentDateTimeUtc = DateTime.UtcNow; // May 21, 2025, 08:12 AM UTC

            // Validate input
            if (dto.StartTime < currentDateTimeUtc)
                throw new ValidationException("Start time cannot be in the past.");

            if (dto.EndTime <= dto.StartTime)
                throw new ValidationException("End time must be after start time.");

            if (dto.EndTime.Subtract(dto.StartTime).TotalMinutes < 30)
                throw new ValidationException("Session must be at least 30 minutes long.");

            if (dto.IsRecurring && string.IsNullOrEmpty(dto.RecurringDay))
                throw new ValidationException("Recurring day must be specified for recurring availability.");

            if (dto.RecurrenceEndDate.HasValue && dto.RecurrenceEndDate.Value < currentDateTimeUtc.Date)
                throw new ValidationException("Recurrence end date cannot be in the past.");

            // Parse recurring day if provided
            DayOfWeek? recurringDay = null;
            if (dto.IsRecurring && !string.IsNullOrEmpty(dto.RecurringDay))
            {
                if (!Enum.TryParse<DayOfWeek>(dto.RecurringDay, true, out var day))
                    throw new ValidationException("Invalid day of week specified.");

                recurringDay = day;
            }

            if (dto.IsRecurring && recurringDay.HasValue)
            {
                var availabilities = GenerateRecurringSlots(tutorId, dto.StartTime, dto.EndTime, recurringDay.Value, dto.RecurrenceEndDate);
                foreach (var availability in availabilities)
                {
                    await _availabilityRepository.AddAsync(availability);
                }
                return MapToDto(availabilities.First()); // Return the first slot as a representative DTO
            }
            else
            {
                var availability = new TutorAvailability
                {
                    AvailabilityId = Guid.NewGuid(),
                    TutorId = tutorId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsRecurring = dto.IsRecurring,
                    RecurringDay = recurringDay,
                    RecurrenceEndDate = dto.RecurrenceEndDate,
                    IsBooked = false
                };

                await _availabilityRepository.AddAsync(availability);
                return MapToDto(availability);
            }
        }

        private List<TutorAvailability> GenerateRecurringSlots(Guid tutorId, DateTime startTime, DateTime endTime, DayOfWeek recurringDay, DateTime? recurrenceEndDate)
        {
            var slots = new List<TutorAvailability>();
            var currentDate = startTime.Date;
            var endDate = recurrenceEndDate ?? DateTime.MaxValue;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == recurringDay)
                {
                    var slotStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, startTime.Hour, startTime.Minute, 0, DateTimeKind.Utc);
                    var slotEnd = slotStart + (endTime - startTime);
                    if (slotStart >= DateTime.UtcNow) // Only include future slots
                    {
                        slots.Add(new TutorAvailability
                        {
                            AvailabilityId = Guid.NewGuid(),
                            TutorId = tutorId,
                            StartTime = slotStart,
                            EndTime = slotEnd,
                            IsRecurring = true,
                            RecurringDay = recurringDay,
                            RecurrenceEndDate = recurrenceEndDate,
                            IsBooked = false
                        });
                    }
                }
                currentDate = currentDate.AddDays(1);
            }

            return slots;
        }

        public async Task<bool> DeleteAsync(Guid availabilityId)
        {
            try
            {
                await _availabilityRepository.DeleteAsync(availabilityId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete availability with ID {AvailabilityId}.", availabilityId);
                return false;
            }
        }

        public async Task<IEnumerable<TutorAvailabilityDto>> GetAvailableSlotsAsync(Guid tutorId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ValidationException("Start date must be before end date.");

            var currentDateTimeUtc = DateTime.UtcNow; // May 21, 2025, 08:12 AM UTC
            if (startDate < currentDateTimeUtc)
                startDate = currentDateTimeUtc;

            var availabilities = await _availabilityRepository.GetAvailableSlotsByTutorIdAsync(tutorId, startDate, endDate);
            return availabilities.Select(MapToDto);
        }

        public async Task<TutorAvailabilityDto> GetByIdAsync(Guid availabilityId)
        {
            var availability = await _availabilityRepository.GetByIdAsync(availabilityId);
            return availability != null ? MapToDto(availability) : null;
        }

        public async Task<IEnumerable<TutorAvailabilityDto>> GetByTutorIdAsync(Guid tutorId)
        {
            var availabilities = await _availabilityRepository.GetByTutorIdAsync(tutorId);
            return availabilities.Select(MapToDto);
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
                RecurringDay = availability.RecurringDay?.ToString(),
                RecurrenceEndDate = availability.RecurrenceEndDate,
                IsBooked = availability.IsBooked
            } : null;
        }
    }
}