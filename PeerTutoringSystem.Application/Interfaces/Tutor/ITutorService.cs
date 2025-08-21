using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.DTOs.Tutor;
using PeerTutoringSystem.Application.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Tutor
{
    public interface ITutorService
    {
        Task<Result<IEnumerable<EnrichedTutorDto>>> GetAllEnrichedTutorsAsync(string? sortBy, int? limit);
        Task<Result<EnrichedTutorDto>> GetEnrichedTutorByIdAsync(string id);
        Task<Result<TutorDashboardStatsDto>> GetTutorDashboardStats();
        Task<Result<TutorFinanceDetailsDto>> GetTutorFinanceDetailsAsync();
    }
}