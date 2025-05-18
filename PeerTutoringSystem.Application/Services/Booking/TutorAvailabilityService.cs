// PeerTutoringSystem.Application/Services/Booking/TutorAvailabilityService.cs
using PeerTutoringSystem.Application.DTOs.Booking;
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
    public class TutorAvailabilityService : ITutorAvailabilityService
    {
        private readonly ITutorAvailabilityRepository _availabilityRepository;

        public TutorAvailabilityService(ITutorAvailabilityRepository availabilityRepository)
        {
            _availabilityRepository = availabilityRepository;
        }

        public async Task<TutorAvailabilityDto> AddAsync(Guid tutorId, CreateTutorAvailabilityDto dto)
        {
            // Validate input
            if (dto.StartTime >= dto.EndTime)
                throw new ValidationException("Start time must be before end time.");

            if (dto.EndTime.Subtract(dto.StartTime).TotalMinutes < 30)
                throw new ValidationException("Session must be at least 30 minutes long.");

            if (dto.IsRecurring && string.IsNullOrEmpty(dto.RecurringDay))
                throw new ValidationException("Recurring day must be specified for recurring availability.");

            // Parse recurring day if provided
            DayOfWeek? recurringDay = null;
            if (dto.IsRecurring && !string.IsNullOrEmpty(dto.RecurringDay))
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
                await _availabilityRepository.DeleteAsync(availabilityId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<TutorAvailabilityDto>> GetAvailableSlotsAsync(Guid tutorId, DateTime startDate, DateTime endDate)
        {
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
            return new TutorAvailabilityDto
            {
                AvailabilityId = availability.AvailabilityId,
                TutorId = availability.TutorId,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                IsRecurring = availability.IsRecurring,
                RecurringDay = availability.RecurringDay?.ToString(),
                RecurrenceEndDate = availability.RecurrenceEndDate,
                IsBooked = availability.IsBooked
            };
        }
    }
}
