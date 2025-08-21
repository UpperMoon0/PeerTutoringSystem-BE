using PeerTutoringSystem.Domain.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Booking
{
    public interface ITutorAvailabilityRepository
    {
        Task<IEnumerable<TutorAvailability>> GetByTutorIdAsync(Guid tutorId);
        Task<(IEnumerable<TutorAvailability> Availabilities, int TotalCount)> GetByTutorIdAsync(Guid tutorId, BookingFilter filter);
        Task<TutorAvailability> GetByIdAsync(Guid availabilityId);
        Task AddAsync(TutorAvailability availability);
        Task UpdateAsync(TutorAvailability availability);
        Task DeleteAsync(Guid availabilityId);
        Task<IEnumerable<TutorAvailability>> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate);
        Task<(IEnumerable<TutorAvailability> Availabilities, int TotalCount)> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate, BookingFilter filter);
    }
}