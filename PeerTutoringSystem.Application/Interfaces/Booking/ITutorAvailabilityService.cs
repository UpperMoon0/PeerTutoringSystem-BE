using PeerTutoringSystem.Application.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Booking
{
    public interface ITutorAvailabilityService
    {
        Task<TutorAvailabilityDto> AddAsync(Guid tutorId, CreateTutorAvailabilityDto dto);
        Task<(IEnumerable<TutorAvailabilityDto> Availabilities, int TotalCount)> GetByTutorIdAsync(Guid tutorId, BookingFilterDto filter);
        Task<TutorAvailabilityDto> GetByIdAsync(Guid availabilityId);
        Task<bool> DeleteAsync(Guid availabilityId);
        Task<(IEnumerable<TutorAvailabilityDto> Availabilities, int TotalCount)> GetAvailableSlotsAsync(Guid tutorId, DateTime startDate, DateTime endDate, BookingFilterDto filter);
    }
}