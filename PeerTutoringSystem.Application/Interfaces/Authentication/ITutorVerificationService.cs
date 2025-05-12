using PeerTutoringSystem.Application.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Authentication
{
    public interface ITutorVerificationService
    {
        Task<Guid> RequestTutorAsync(Guid userId, RequestTutorDto dto);
        Task<IEnumerable<TutorVerificationDto>> GetAllVerificationsAsync();
        Task<TutorVerificationDto> GetVerificationByIdAsync(Guid verificationId);
        Task UpdateVerificationAsync(Guid verificationId, UpdateTutorVerificationDto dto);
    }
}