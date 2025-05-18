// PeerTutoringSystem.Infrastructure/Repositories/Booking/TutorAvailabilityRepository.cs
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
            _context = context;
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

        public async Task UpdateAsync(TutorAvailability availability)
        {
            _context.TutorAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TutorAvailability>> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate)
        {
            // Get all non-recurring slots within date range
            var nonRecurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           !a.IsRecurring &&
                           !a.IsBooked &&
                           a.StartTime >= startDate &&
                           a.StartTime <= endDate)
                .ToListAsync();

            // Get recurring slots that apply to the date range
            var recurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           a.IsRecurring &&
                           !a.IsBooked &&
                           (a.RecurrenceEndDate == null || a.RecurrenceEndDate >= startDate))
                .ToListAsync();

            // Filter recurring slots by checking if they fall within the requested date range
            var filteredRecurringSlots = new List<TutorAvailability>();
            foreach (var slot in recurringSlots)
            {
                // Create instances of recurring slots that fall within requested date range
                var instances = GenerateRecurringInstances(slot, startDate, endDate);
                filteredRecurringSlots.AddRange(instances);
            }

            // Combine both sets and return
            return nonRecurringSlots.Concat(filteredRecurringSlots)
                .OrderBy(a => a.StartTime)
                .ToList();
        }

        private List<TutorAvailability> GenerateRecurringInstances(TutorAvailability recurring, DateTime startDate, DateTime endDate)
        {
            var instances = new List<TutorAvailability>();
            if (!recurring.RecurringDay.HasValue) return instances;

            var day = recurring.RecurringDay.Value;
            var currentDate = startDate.Date;

            // Find first occurrence after start date
            while (currentDate.DayOfWeek != day)
            {
                currentDate = currentDate.AddDays(1);
            }

            // Generate all occurrences within the date range
            while (currentDate <= endDate &&
                  (recurring.RecurrenceEndDate == null || currentDate <= recurring.RecurrenceEndDate))
            {
                // Calculate time for this instance
                var startTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    recurring.StartTime.Hour,
                    recurring.StartTime.Minute,
                    0);

                var endTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    recurring.EndTime.Hour,
                    recurring.EndTime.Minute,
                    0);

                // Check if this specific instance isn't already booked
                var isBooked = IsTimeSlotBooked(recurring.TutorId, startTime, endTime).Result;

                if (!isBooked)
                {
                    // Create a new instance with the same properties but specific date/time
                    instances.Add(new TutorAvailability
                    {
                        AvailabilityId = recurring.AvailabilityId, // Keep reference to original
                        TutorId = recurring.TutorId,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsRecurring = false, // Set to false for the instance
                        IsBooked = false
                    });
                }

                // Move to next week
                currentDate = currentDate.AddDays(7);
            }

            return instances;
        }

        private async Task<bool> IsTimeSlotBooked(Guid tutorId, DateTime startTime, DateTime endTime)
        {
            // Check if there's an existing booking that would make this time slot unavailable
            return await _context.BookingSessions
                .Where(b => b.TutorId == tutorId &&
                           b.Status != BookingStatus.Cancelled &&
                           ((b.StartTime <= startTime && b.EndTime > startTime) ||
                            (b.StartTime < endTime && b.EndTime >= endTime) ||
                            (b.StartTime >= startTime && b.EndTime <= endTime)))
                .AnyAsync();
        }
    }
}
