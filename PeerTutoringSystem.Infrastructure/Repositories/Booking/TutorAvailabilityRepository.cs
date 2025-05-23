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
            var nonRecurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           !a.IsRecurring &&
                           !a.IsBooked &&
                           a.StartTime >= startDate &&
                           a.StartTime <= endDate)
                .ToListAsync();

            var recurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           a.IsRecurring &&
                           !a.IsBooked &&
                           (a.RecurrenceEndDate == null || a.RecurrenceEndDate >= startDate))
                .ToListAsync();

            var filteredRecurringSlots = new List<TutorAvailability>();
            foreach (var slot in recurringSlots)
            {
                var instances = GenerateRecurringInstances(slot, startDate, endDate);
                filteredRecurringSlots.AddRange(instances);
            }

            return nonRecurringSlots.Concat(filteredRecurringSlots)
                .OrderBy(a => a.StartTime)
                .ToList();
        }

        public async Task<(IEnumerable<TutorAvailability> Availabilities, int TotalCount)> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate, BookingFilter filter)
        {
            var query = _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           !a.IsBooked &&
                           a.StartTime >= startDate &&
                           a.StartTime <= endDate);

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.EndTime <= filter.EndDate.Value);
            }

            var nonRecurringSlots = await query
                .Where(a => !a.IsRecurring)
                .ToListAsync();

            var recurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           a.IsRecurring &&
                           !a.IsBooked &&
                           (a.RecurrenceEndDate == null || a.RecurrenceEndDate >= startDate))
                .ToListAsync();

            var filteredRecurringSlots = new List<TutorAvailability>();
            foreach (var slot in recurringSlots)
            {
                var instances = GenerateRecurringInstances(slot, startDate, endDate);
                filteredRecurringSlots.AddRange(instances);
            }

            var allSlots = nonRecurringSlots.Concat(filteredRecurringSlots)
                .OrderBy(a => a.StartTime)
                .ToList();

            var totalCount = allSlots.Count;
            var pagedSlots = allSlots
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return (pagedSlots, totalCount);
        }

        private List<TutorAvailability> GenerateRecurringInstances(TutorAvailability recurring, DateTime startDate, DateTime endDate)
        {
            var instances = new List<TutorAvailability>();
            if (!recurring.RecurringDay.HasValue) return instances;

            var day = recurring.RecurringDay.Value;
            var currentDate = startDate.Date;

            while (currentDate.DayOfWeek != day)
            {
                currentDate = currentDate.AddDays(1);
            }

            while (currentDate <= endDate &&
                  (recurring.RecurrenceEndDate == null || currentDate <= recurring.RecurrenceEndDate))
            {
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

                var isBooked = IsTimeSlotBooked(recurring.TutorId, startTime, endTime).Result;

                if (!isBooked)
                {
                    instances.Add(new TutorAvailability
                    {
                        AvailabilityId = recurring.AvailabilityId,
                        TutorId = recurring.TutorId,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsRecurring = false,
                        IsBooked = false
                    });
                }

                currentDate = currentDate.AddDays(7);
            }

            return instances;
        }

        private async Task<bool> IsTimeSlotBooked(Guid tutorId, DateTime startTime, DateTime endTime)
        {
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