using PeerTutoringSystem.Application.DTOs.Booking;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface ITutorAvailabilityService
    {
        Task<TutorAvailabilityDto> AddAsync(Guid tutorId, CreateTutorAvailabilityDto dto);
        Task<(IEnumerable<TutorAvailabilityDto> Availabilities, int TotalCount)> GetByTutorIdAsync(Guid tutorId, BookingFilterDto filter);
        Task<TutorAvailabilityDto> GetByIdAsync(Guid availabilityId);
        Task<bool> DeleteAsync(Guid availabilityId);
        Task<(IEnumerable<TutorAvailabilityDto> Availabilities, int TotalCount)> GetAvailableSlotsAsync(Guid tutorId, DateTime startDate, DateTime endDate, string? status, BookingFilterDto filter);
        Task<TutorAvailabilityDto> UpdateAsync(Guid availabilityId, UpdateTutorAvailabilityDto dto);
    }
}