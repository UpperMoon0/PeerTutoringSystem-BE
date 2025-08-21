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

            // Get all non-booked availabilities for the tutor that fall within the time range.
            // This simplified logic correctly handles both recurring and non-recurring slots
            // by relying on the IsBooked flag, which should be the single source of truth.
            var availabilities = await _context.TutorAvailabilities
                .Where(a => a.TutorId == tutorId &&
                           !a.IsBooked &&
                           a.StartTime < endDate && a.EndTime > startDate)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return availabilities;
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

    }
}