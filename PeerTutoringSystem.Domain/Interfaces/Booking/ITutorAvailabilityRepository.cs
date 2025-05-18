// PeerTutoringSystem.Domain/Interfaces/Booking/ITutorAvailabilityRepository.cs
using PeerTutoringSystem.Domain.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Booking
{
    public interface ITutorAvailabilityRepository
    {
        Task<IEnumerable<TutorAvailability>> GetByTutorIdAsync(Guid tutorId);
        Task<TutorAvailability> GetByIdAsync(Guid availabilityId);
        Task AddAsync(TutorAvailability availability);
        Task UpdateAsync(TutorAvailability availability);
        Task DeleteAsync(Guid availabilityId);
        Task<IEnumerable<TutorAvailability>> GetAvailableSlotsByTutorIdAsync(Guid tutorId, DateTime startDate, DateTime endDate);
    }
}