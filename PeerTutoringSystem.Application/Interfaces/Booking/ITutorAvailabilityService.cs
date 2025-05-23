using PeerTutoringSystem.Application.DTOs.Booking;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface ITutorAvailabilityService
    {
        Task<TutorAvailabilityDto> AddAsync(Guid tutorId, CreateTutorAvailabilityDto dto);
        Task<IEnumerable<TutorAvailabilityDto>> GetByTutorIdAsync(Guid tutorId);
        Task<TutorAvailabilityDto> GetByIdAsync(Guid availabilityId);
        Task<bool> DeleteAsync(Guid availabilityId);
        Task<IEnumerable<TutorAvailabilityDto>> GetAvailableSlotsAsync(Guid tutorId, DateTime startDate, DateTime endDate);
    }
}