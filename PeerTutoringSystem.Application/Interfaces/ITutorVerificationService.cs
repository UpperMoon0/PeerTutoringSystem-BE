using PeerTutoringSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces
{
    public interface ITutorVerificationService
    {
        Task<Guid> RequestTutorAsync(Guid userId, RequestTutorDto dto);
        Task<IEnumerable<TutorVerificationDto>> GetAllVerificationsAsync();
        Task<TutorVerificationDto> GetVerificationByIdAsync(Guid verificationId);
        Task UpdateVerificationAsync(Guid verificationId, UpdateTutorVerificationDto dto);
    }
}