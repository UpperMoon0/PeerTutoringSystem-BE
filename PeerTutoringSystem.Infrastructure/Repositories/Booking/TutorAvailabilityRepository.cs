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
            // Đảm bảo startDate và endDate là UTC
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            // Lấy các slot không lặp lại
            var nonRecurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           !a.IsRecurring &&
                           !a.IsDailyRecurring &&
                           !a.IsBooked &&
                           a.StartTime >= startDate &&
                           a.StartTime <= endDate)
                .ToListAsync();

            // Lấy các slot lặp lại hàng tuần
            var weeklyRecurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           a.IsRecurring &&
                           !a.IsDailyRecurring &&
                           !a.IsBooked &&
                           (a.RecurrenceEndDate == null || a.RecurrenceEndDate >= startDate))
                .ToListAsync();

            // Lấy các slot lặp lại hàng ngày
            var dailyRecurringSlots = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           a.IsDailyRecurring &&
                           !a.IsBooked &&
                           (a.RecurrenceEndDate == null || a.RecurrenceEndDate >= startDate))
                .ToListAsync();

            var filteredRecurringSlots = new List<TutorAvailability>();

            // Xử lý slot lặp lại hàng tuần
            foreach (var slot in weeklyRecurringSlots)
            {
                var instances = GenerateWeeklyRecurringInstances(slot, startDate, endDate);
                filteredRecurringSlots.AddRange(instances);
            }

            // Xử lý slot lặp lại hàng ngày
            foreach (var slot in dailyRecurringSlots)
            {
                var instances = GenerateDailyRecurringInstances(slot, startDate, endDate);
                filteredRecurringSlots.AddRange(instances);
            }

            return nonRecurringSlots.Concat(filteredRecurringSlots)
                .OrderBy(a => a.StartTime)
                .ToList();
        }

        public async Task<(IEnumerable<TutorAvailability> Availabilities, int TotalCount)> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate, BookingFilter filter)
        {
            // Đảm bảo startDate và endDate là UTC
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            // Lấy tất cả slot khả dụng
            var allSlots = await GetAvailableSlotsByTutorIdAsync(tutorId, startDate, endDate);

            // Áp dụng bộ lọc
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

        private List<TutorAvailability> GenerateWeeklyRecurringInstances(TutorAvailability recurring, DateTime startDate, DateTime endDate)
        {
            var instances = new List<TutorAvailability>();
            if (!recurring.RecurringDay.HasValue)
                return instances;

            var day = recurring.RecurringDay.Value;
            var currentDate = startDate.Date;
            var recurrenceEnd = recurring.RecurrenceEndDate?.ToUniversalTime() ?? endDate;

            // Tìm ngày đầu tiên phù hợp với RecurringDay
            while (currentDate <= endDate && currentDate.DayOfWeek != day)
            {
                currentDate = currentDate.AddDays(1);
            }

            while (currentDate <= endDate && currentDate <= recurrenceEnd)
            {
                var startTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    recurring.StartTime.Hour,
                    recurring.StartTime.Minute,
                    0,
                    DateTimeKind.Utc);

                var endTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    recurring.EndTime.Hour,
                    recurring.EndTime.Minute,
                    0,
                    DateTimeKind.Utc);

                if (startTime >= DateTime.UtcNow && !IsTimeSlotBooked(recurring.TutorId, startTime, endTime).Result)
                {
                    instances.Add(new TutorAvailability
                    {
                        AvailabilityId = recurring.AvailabilityId,
                        TutorId = recurring.TutorId,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsRecurring = false,
                        IsDailyRecurring = false,
                        IsBooked = false
                    });
                }

                currentDate = currentDate.AddDays(7);
            }

            return instances;
        }

        private List<TutorAvailability> GenerateDailyRecurringInstances(TutorAvailability recurring, DateTime startDate, DateTime endDate)
        {
            var instances = new List<TutorAvailability>();
            var currentDate = startDate.Date;
            var recurrenceEnd = recurring.RecurrenceEndDate?.ToUniversalTime() ?? endDate;

            while (currentDate <= endDate && currentDate <= recurrenceEnd)
            {
                var startTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    recurring.StartTime.Hour,
                    recurring.StartTime.Minute,
                    0,
                    DateTimeKind.Utc);

                var endTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    recurring.EndTime.Hour,
                    recurring.EndTime.Minute,
                    0,
                    DateTimeKind.Utc);

                if (startTime >= DateTime.UtcNow && !IsTimeSlotBooked(recurring.TutorId, startTime, endTime).Result)
                {
                    instances.Add(new TutorAvailability
                    {
                        AvailabilityId = recurring.AvailabilityId,
                        TutorId = recurring.TutorId,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsRecurring = false,
                        IsDailyRecurring = false,
                        IsBooked = false
                    });
                }

                currentDate = currentDate.AddDays(1);
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