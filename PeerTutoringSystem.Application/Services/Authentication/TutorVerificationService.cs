using PeerTutoringSystem.Application.DTOs.Authentication;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace PeerTutoringSystem.Application.Services.Authentication
{
    public class TutorVerificationService : ITutorVerificationService
    {
        private readonly ITutorVerificationRepository _tutorVerificationRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TutorVerificationService> _logger;

        public TutorVerificationService(
            ITutorVerificationRepository tutorVerificationRepository,
            IDocumentRepository documentRepository,
            IUserRepository userRepository,
            ILogger<TutorVerificationService> logger)
        {
            _tutorVerificationRepository = tutorVerificationRepository;
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Guid> RequestTutorAsync(Guid userId, RequestTutorDto dto)
        {
            ValidateDto(dto);
            _logger.LogInformation("Starting tutor verification request for user ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
            {
                _logger.LogWarning("User not found or inactive for user ID: {UserId}", userId);
                throw new ValidationException("User not found or inactive.");
            }
            if (user.Role.RoleName != "Student")
            {
                _logger.LogWarning("User with ID: {UserId} is not a student. Role: {Role}", userId, user.Role.RoleName);
                throw new ValidationException("Only students can request tutor role.");
            }

            var existingVerifications = await GetVerificationsByUserIdAsync(userId);
            if (existingVerifications.Any())
            {
                var latestVerification = existingVerifications
                    .OrderByDescending(v => v.VerificationDate ?? DateTime.MinValue)
                    .First();

                if (latestVerification.VerificationStatus == "Pending")
                {
                    _logger.LogWarning("User ID: {UserId} already has a pending tutor verification request.", userId);
                    throw new ValidationException("You already have a pending tutor verification request.");
                }
                if (latestVerification.VerificationStatus == "Approved")
                {
                    _logger.LogWarning("User ID: {UserId} is already a tutor.", userId);
                    throw new ValidationException("You are already a tutor. No need to request again.");
                }
                _logger.LogInformation("Previous request for user ID: {UserId} was rejected. Allowing new request.", userId);
            }

            var verification = new TutorVerification
            {
                VerificationID = Guid.NewGuid(),
                UserID = userId,
                CitizenID = dto.CitizenID,
                StudentID = dto.StudentID,
                University = dto.University,
                Major = dto.Major,
                VerificationStatus = "Pending",
                AccessLevel = "Tutor",
                VerificationDate = DateTime.UtcNow
            };
            await _tutorVerificationRepository.AddAsync(verification);
            _logger.LogInformation("Created tutor verification request with ID: {VerificationId} for user ID: {UserId}", verification.VerificationID, userId);

            foreach (var doc in dto.Documents)
            {
                // Document path is now a Firebase Storage URL, no need to check local existence
                // The DocumentService already handles the upload to Firebase Storage.
                // We just need to ensure the DocumentPath is stored correctly.

                var document = new Document
                {
                    DocumentID = Guid.NewGuid(),
                    VerificationID = verification.VerificationID,
                    DocumentType = doc.DocumentType,
                    DocumentPath = doc.DocumentPath,
                    FileSize = doc.FileSize,
                    UploadDate = DateTime.UtcNow,
                    AccessLevel = "Tutor"
                };
                await _documentRepository.AddAsync(document);
                _logger.LogInformation("Added document with ID: {DocumentId} for verification ID: {VerificationId}", document.DocumentID, verification.VerificationID);
            }

            return verification.VerificationID;
        }

        public async Task<IEnumerable<TutorVerificationDto>> GetVerificationsByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Fetching tutor verifications for user ID: {UserId}", userId);
            var verifications = await _tutorVerificationRepository.GetByUserIdAsync(userId);
            var result = new List<TutorVerificationDto>();

            foreach (var verification in verifications)
            {
                var documents = await _documentRepository.GetByVerificationIdAsync(verification.VerificationID);
                var user = await _userRepository.GetByIdAsync(verification.UserID);
                result.Add(new TutorVerificationDto
                {
                    VerificationID = verification.VerificationID,
                    UserID = verification.UserID,
                    FullName = user?.FullName ?? "N/A",
                    Email = user?.Email ?? "N/A",
                    Avatar = user?.AvatarUrl ?? "N/A",
                    CitizenID = verification.CitizenID,
                    StudentID = verification.StudentID,
                    University = verification.University,
                    Major = verification.Major,
                    VerificationStatus = verification.VerificationStatus,
                    VerificationDate = verification.VerificationDate,
                    AdminNotes = verification.AdminNotes,
                    Documents = documents.Select(d => new DocumentResponseDto
                    {
                        DocumentID = d.DocumentID,
                        DocumentType = d.DocumentType,
                        DocumentPath = d.DocumentPath,
                        UploadDate = d.UploadDate,
                        FileSize = d.FileSize
                    }).ToList()
                });
            }

            _logger.LogInformation("Found {Count} tutor verifications for user ID: {UserId}", result.Count, userId);
            return result;
        }

        public async Task<IEnumerable<TutorVerificationDto>> GetAllVerificationsAsync()
        {
            _logger.LogInformation("Fetching all tutor verifications");
            var verifications = await _tutorVerificationRepository.GetAllAsync();
            var result = new List<TutorVerificationDto>();

            foreach (var verification in verifications)
            {
                var documents = await _documentRepository.GetByVerificationIdAsync(verification.VerificationID);
                var user = await _userRepository.GetByIdAsync(verification.UserID);
                result.Add(new TutorVerificationDto
                {
                    VerificationID = verification.VerificationID,
                    UserID = verification.UserID,
                    FullName = user?.FullName ?? "N/A",
                    Email = user?.Email ?? "N/A",
                    Avatar = user?.AvatarUrl ?? "N/A",
                    CitizenID = verification.CitizenID,
                    StudentID = verification.StudentID,
                    University = verification.University,
                    Major = verification.Major,
                    VerificationStatus = verification.VerificationStatus,
                    VerificationDate = verification.VerificationDate,
                    AdminNotes = verification.AdminNotes,
                    Documents = documents.Select(d => new DocumentResponseDto
                    {
                        DocumentID = d.DocumentID,
                        DocumentType = d.DocumentType,
                        DocumentPath = d.DocumentPath,
                        UploadDate = d.UploadDate,
                        FileSize = d.FileSize
                    }).ToList()
                });
            }

            _logger.LogInformation("Fetched {Count} tutor verifications", result.Count);
            return result;
        }

        public async Task<TutorVerificationDto> GetVerificationByIdAsync(Guid verificationId)
        {
            _logger.LogInformation("Fetching tutor verification with ID: {VerificationId}", verificationId);
            var verification = await _tutorVerificationRepository.GetByIdAsync(verificationId);
            if (verification == null)
            {
                _logger.LogWarning("Tutor verification with ID: {VerificationId} not found", verificationId);
                throw new ValidationException("Verification request not found.");
            }

            var documents = await _documentRepository.GetByVerificationIdAsync(verificationId);
            var user = await _userRepository.GetByIdAsync(verification.UserID);
            var result = new TutorVerificationDto
            {
                VerificationID = verification.VerificationID,
                UserID = verification.UserID,
                FullName = user?.FullName ?? "N/A",
                Email = user?.Email ?? "N/A",
                Avatar = user?.AvatarUrl ?? "N/A",
                CitizenID = verification.CitizenID,
                StudentID = verification.StudentID,
                University = verification.University,
                Major = verification.Major,
                VerificationStatus = verification.VerificationStatus,
                VerificationDate = verification.VerificationDate,
                AdminNotes = verification.AdminNotes,
                Documents = documents.Select(d => new DocumentResponseDto
                {
                    DocumentID = d.DocumentID,
                    DocumentType = d.DocumentType,
                    DocumentPath = d.DocumentPath,
                    UploadDate = d.UploadDate,
                    FileSize = d.FileSize
                }).ToList()
            };

            _logger.LogInformation("Successfully fetched tutor verification with ID: {VerificationId}", verificationId);
            return result;
        }

        public async Task UpdateVerificationAsync(Guid verificationId, UpdateTutorVerificationDto dto)
        {
            _logger.LogInformation("Updating tutor verification with ID: {VerificationId}", verificationId);
            ValidateDto(dto);

            var verification = await _tutorVerificationRepository.GetByIdAsync(verificationId);
            if (verification == null)
            {
                _logger.LogWarning("Tutor verification with ID: {VerificationId} not found", verificationId);
                throw new ValidationException("Verification request not found.");
            }

            verification.VerificationStatus = dto.VerificationStatus;
            verification.AdminNotes = dto.AdminNotes;
            verification.VerificationDate = DateTime.UtcNow;

            if (dto.VerificationStatus == "Approved")
            {
                if (verification.User != null)
                {
                    verification.User.RoleID = 2;
                    await _userRepository.UpdateAsync(verification.User);
                    _logger.LogInformation("User ID: {UserId} role updated to Tutor", verification.User.UserID);
                }
                else
                {
                    _logger.LogWarning("User ID: {UserId} not found for verification ID: {VerificationId}", verification.UserID, verificationId);
                }
            }

            await _tutorVerificationRepository.UpdateAsync(verification);
            _logger.LogInformation("Successfully updated tutor verification with ID: {VerificationId}", verificationId);
        }

        private void ValidateDto<T>(T dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                _logger.LogWarning("Validation failed for DTO: {Errors}", errors);
                throw new ValidationException(errors);
            }
        }

        public async Task<(bool HasVerification, string LatestStatus)> HasPendingOrApprovedTutorVerificationAsync(Guid userId, params string[] statuses)
        {
            _logger.LogInformation("Checking tutor verification status for user ID: {UserId} with statuses: {Statuses}", userId, string.Join(", ", statuses));

            var latestVerification = await _tutorVerificationRepository.GetLatestVerificationAsync(userId);
            if (latestVerification == null)
            {
                _logger.LogInformation("No verification found for user ID: {UserId}", userId);
                return (false, "None");
            }

            var hasVerification = statuses.Contains(latestVerification.VerificationStatus);
            _logger.LogInformation("User ID: {UserId} has verification status: {Status}, result: {HasVerification}", userId, latestVerification.VerificationStatus, hasVerification);

            return (hasVerification, latestVerification.VerificationStatus);
        }
    }
}