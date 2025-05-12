using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.Interfaces;
using PeerTutoringSystem.Domain.Entities;
using PeerTutoringSystem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services
{
    public class TutorVerificationService : ITutorVerificationService
    {
        private readonly ITutorVerificationRepository _tutorVerificationRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUserRepository _userRepository;

        public TutorVerificationService(
            ITutorVerificationRepository tutorVerificationRepository,
            IDocumentRepository documentRepository,
            IUserRepository userRepository)
        {
            _tutorVerificationRepository = tutorVerificationRepository;
            _documentRepository = documentRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> RequestTutorAsync(Guid userId, RequestTutorDto dto)
        {
            // Validate DTO
            ValidateDto(dto);

            // Kiểm tra người dùng
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
                throw new ValidationException("User not found or inactive.");
            if (user.Role.RoleName != "Student")
                throw new ValidationException("Only students can request tutor role.");

            // Tạo bản ghi TutorVerification
            var verification = new TutorVerification
            {
                VerificationID = Guid.NewGuid(),
                UserID = userId,
                CitizenID = dto.CitizenID,
                StudentID = dto.StudentID,
                University = dto.University,
                Major = dto.Major,
                VerificationStatus = "Pending",
                AccessLevel = "Tutor"
            };
            await _tutorVerificationRepository.AddAsync(verification);

            // Xử lý tài liệu (không cần lưu tệp, chỉ tạo bản ghi Document)
            foreach (var doc in dto.Documents)
            {
                // Kiểm tra DocumentPath tồn tại
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.DocumentPath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                    throw new ValidationException($"Document at path {doc.DocumentPath} does not exist on server.");

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
            }

            return verification.VerificationID;
        }

        // Các phương thức khác giữ nguyên
        public async Task<IEnumerable<TutorVerificationDto>> GetAllVerificationsAsync()
        {
            var verifications = await _tutorVerificationRepository.GetAllAsync();
            var result = new List<TutorVerificationDto>();

            foreach (var verification in verifications)
            {
                var documents = await _documentRepository.GetByVerificationIdAsync(verification.VerificationID);
                result.Add(new TutorVerificationDto
                {
                    VerificationID = verification.VerificationID,
                    UserID = verification.UserID,
                    CitizenID = verification.CitizenID,
                    StudentID = verification.StudentID,
                    University = verification.University,
                    Major = verification.Major,
                    VerificationStatus = verification.VerificationStatus,
                    VerificationDate = verification.VerificationDate,
                    AdminNotes = verification.AdminNotes,
                    Documents = documents.Select(d => new DocumentDto
                    {
                        DocumentID = d.DocumentID,
                        DocumentType = d.DocumentType,
                        DocumentPath = d.DocumentPath,
                        UploadDate = d.UploadDate,
                        FileSize = d.FileSize
                    }).ToList()
                });
            }

            return result;
        }

        public async Task<TutorVerificationDto> GetVerificationByIdAsync(Guid verificationId)
        {
            var verification = await _tutorVerificationRepository.GetByIdAsync(verificationId);
            if (verification == null)
                throw new ValidationException("Verification request not found.");

            var documents = await _documentRepository.GetByVerificationIdAsync(verificationId);
            return new TutorVerificationDto
            {
                VerificationID = verification.VerificationID,
                UserID = verification.UserID,
                CitizenID = verification.CitizenID,
                StudentID = verification.StudentID,
                University = verification.University,
                Major = verification.Major,
                VerificationStatus = verification.VerificationStatus,
                VerificationDate = verification.VerificationDate,
                AdminNotes = verification.AdminNotes,
                Documents = documents.Select(d => new DocumentDto
                {
                    DocumentID = d.DocumentID,
                    DocumentType = d.DocumentType,
                    DocumentPath = d.DocumentPath,
                    UploadDate = d.UploadDate,
                    FileSize = d.FileSize
                }).ToList()
            };
        }

        public async Task UpdateVerificationAsync(Guid verificationId, UpdateTutorVerificationDto dto)
        {
            ValidateDto(dto);

            var verification = await _tutorVerificationRepository.GetByIdAsync(verificationId);
            if (verification == null)
                throw new ValidationException("Verification request not found.");

            verification.VerificationStatus = dto.VerificationStatus;
            verification.AdminNotes = dto.AdminNotes;
            verification.VerificationDate = DateTime.UtcNow;

            if (dto.VerificationStatus == "Approved")
            {
                var user = await _userRepository.GetByIdAsync(verification.UserID);
                if (user != null)
                {
                    user.RoleID = 2; // Tutor
                    await _userRepository.UpdateAsync(user);
                }
            }

            await _tutorVerificationRepository.UpdateAsync(verification);
        }

        private void ValidateDto<T>(T dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}