using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Tutor
{
    public interface ITutorService
    {
        Task<Result<IEnumerable<EnrichedTutorDto>>> GetAllEnrichedTutorsAsync();
    }
}