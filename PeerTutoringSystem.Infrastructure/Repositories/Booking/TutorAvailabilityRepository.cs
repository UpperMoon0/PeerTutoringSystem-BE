using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Booking
{
    public class TutorAvailabilityRepository : ITutorAvailabilityRepository
    {
        private readonly AppDbContext _context;

        public TutorAvailabilityRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(TutorAvailability availability)
        {
            await _context.TutorAvailabilities.AddAsync(availability);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid availabilityId)
        {
            var availability = await _context.TutorAvailabilities.FindAsync(availabilityId);
            if (availability != null)
            {
                _context.TutorAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TutorAvailability> GetByIdAsync(Guid availabilityId)
        {
            return await _context.TutorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);
        }

        public async Task<IEnumerable<TutorAvailability>> GetByTutorIdAsync(Guid tutorId)
        {
            return await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<(IEnumerable<TutorAvailability> Availabilities, int TotalCount)> GetByTutorIdAsync(Guid tutorId, BookingFilter filter)
        {
            var query = _context.TutorAvailabilities.Where(a => a.TutorId == tutorId);

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.EndTime <= filter.EndDate.Value);
            }

            var totalCount = await query.CountAsync();
            var availabilities = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return (availabilities, totalCount);
        }

        public async Task UpdateAsync(TutorAvailability availability)
        {
            _context.TutorAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TutorAvailability>> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate)
        {
            // Ensure startDate and endDate are in UTC
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            // Get all non-booked availabilities for the tutor
            var availabilities = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           !a.IsBooked &&
                           (a.StartTime <= endDate &&
                            (a.RecurrenceEndDate == null || a.RecurrenceEndDate >= startDate)))
                .ToListAsync();

            var result = new List<TutorAvailability>();

            foreach (var availability in availabilities)
            {
                // For non-recurring slots, check if they fall within the requested time range
                if (!availability.IsRecurring && !availability.IsDailyRecurring)
                {
                    if (availability.StartTime >= startDate && availability.StartTime <= endDate)
                    {
                        result.Add(availability);
                    }
                }
                // For recurring slots (weekly or daily), return the original slot if it matches the criteria
                else
                {
                    bool isValidSlot = false;

                    if (availability.IsDailyRecurring)
                    {
                        // Daily recurring: check if the time range overlaps with the requested period
                        isValidSlot = startDate.Date <= (availability.RecurrenceEndDate?.Date ?? endDate.Date);
                    }
                    else if (availability.IsRecurring && availability.RecurringDay.HasValue)
                    {
                        // Weekly recurring: check if any occurrence falls within the requested time range
                        var currentDate = startDate.Date;
                        var recurrenceEnd = availability.RecurrenceEndDate?.ToUniversalTime() ?? endDate;

                        while (currentDate <= endDate && currentDate <= recurrenceEnd)
                        {
                            if (currentDate.DayOfWeek == availability.RecurringDay.Value)
                            {
                                var startTime = new DateTime(
                                    currentDate.Year,
                                    currentDate.Month,
                                    currentDate.Day,
                                    availability.StartTime.Hour,
                                    availability.StartTime.Minute,
                                    0,
                                    DateTimeKind.Utc);

                                if (startTime >= startDate && startTime <= endDate && startTime >= DateTime.UtcNow)
                                {
                                    isValidSlot = true;
                                    break;
                                }
                            }
                            currentDate = currentDate.AddDays(1);
                        }
                    }

                    if (isValidSlot && !await IsTimeSlotBooked(availability.TutorId, availability.StartTime, availability.EndTime))
                    {
                        result.Add(availability);
                    }
                }
            }

            return result.OrderBy(a => a.StartTime).ToList();
        }

        public async Task<(IEnumerable<TutorAvailability> Availabilities, int TotalCount)> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate, BookingFilter filter)
        {
            // Ensure startDate and endDate are in UTC
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            // Get all available slots
            var allSlots = await GetAvailableSlotsByTutorIdAsync(tutorId, startDate, endDate);

            // Apply additional filters
            var query = allSlots.AsQueryable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= filter.StartDate.Value.ToUniversalTime());
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.EndTime <= filter.EndDate.Value.ToUniversalTime());
            }

            var totalCount = query.Count();
            var pagedSlots = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return (pagedSlots, totalCount);
        }

        private async Task<bool> IsTimeSlotBooked(Guid tutorId, DateTime startTime, DateTime endTime)
        {
            return await _context.BookingSessions
                .Where(b => b.TutorId == tutorId &&
                           b.Status != PeerTutoringSystem.Domain.Entities.Booking.BookingStatus.Cancelled &&
                           ((b.StartTime <= startTime && b.EndTime > startTime) ||
                            (b.StartTime < endTime && b.EndTime >= endTime) ||
                            (b.StartTime >= startTime && b.EndTime <= endTime)))
                .AnyAsync();
        }
    }
}