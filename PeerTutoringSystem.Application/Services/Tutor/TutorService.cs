using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.Helpers;
using PeerTutoringSystem.Application.Interfaces.Reviews;
using PeerTutoringSystem.Application.Interfaces.Skills;
using PeerTutoringSystem.Application.Interfaces.Tutor;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;

namespace PeerTutoringSystem.Application.Services.Tutor
{
    public class TutorService : ITutorService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserBioRepository _userBioRepository;
        private readonly IUserSkillService _userSkillService;
        private readonly IReviewService _reviewService;

        public TutorService(
            IUserRepository userRepository,
            IUserBioRepository userBioRepository,
            IUserSkillService userSkillService,
            IReviewService reviewService)
        {
            _userRepository = userRepository;
            _userBioRepository = userBioRepository;
            _userSkillService = userSkillService;
            _reviewService = reviewService;
        }

        public async Task<Result<IEnumerable<EnrichedTutorDto>>> GetAllEnrichedTutorsAsync(string? sortBy, int? limit)
        {
            try
            {
                var tutors = await _userRepository.GetUsersByRoleAsync("Tutor");
                if (tutors == null || !tutors.Any())
                {
                    return Result<IEnumerable<EnrichedTutorDto>>.Success(Enumerable.Empty<EnrichedTutorDto>());
                }

                var enrichedTutors = new List<EnrichedTutorDto>();
                foreach (var tutor in tutors)
                {
                    var userBio = await _userBioRepository.GetByUserIdAsync(tutor.UserID);
                    var skills = await _userSkillService.GetByUserIdAsync(tutor.UserID);
                    var averageRating = await _reviewService.GetAverageRatingByTutorIdAsync(tutor.UserID);
                    var reviewCount = (await _reviewService.GetReviewsByTutorIdAsync(tutor.UserID)).Count();

                    enrichedTutors.Add(new EnrichedTutorDto
                    {
                        UserID = tutor.UserID,
                        FullName = tutor.FullName,
                        Email = tutor.Email,
                        AvatarUrl = tutor.AvatarUrl,
                        Bio = userBio?.Bio ?? string.Empty,
                        Experience = userBio?.Experience ?? string.Empty,
                        HourlyRate = userBio?.HourlyRate ?? 0,
                        Availability = userBio?.Availability ?? string.Empty,
                        School = tutor.School,
                        AverageRating = averageRating,
                        ReviewCount = reviewCount,
                        Skills = skills ?? Enumerable.Empty<UserSkillDto>()
                    });
                }

                IEnumerable<EnrichedTutorDto> finalTutors = enrichedTutors.Where(t => !string.IsNullOrEmpty(t.Bio));

                if (!string.IsNullOrEmpty(sortBy) && sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
                {
                    finalTutors = finalTutors.OrderByDescending(t => t.AverageRating);
                }

                if (limit.HasValue && limit.Value > 0)
                {
                    finalTutors = finalTutors.Take(limit.Value);
                }

                return Result<IEnumerable<EnrichedTutorDto>>.Success(finalTutors);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<EnrichedTutorDto>>.Failure($"Failed to retrieve enriched tutor data: {ex.Message}");
            }
        }
        public async Task<Result<EnrichedTutorDto>> GetEnrichedTutorByIdAsync(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var userId))
                {
                    return Result<EnrichedTutorDto>.Failure("Invalid user ID format.");
                }

                var tutor = await _userRepository.GetByIdAsync(userId);
                if (tutor == null || tutor.Role.RoleName != "Tutor")
                {
                    return Result<EnrichedTutorDto>.Failure("Tutor not found.");
                }

                var userBio = await _userBioRepository.GetByUserIdAsync(tutor.UserID);
                var skills = await _userSkillService.GetByUserIdAsync(tutor.UserID);
                var averageRating = await _reviewService.GetAverageRatingByTutorIdAsync(tutor.UserID);
                var reviewCount = (await _reviewService.GetReviewsByTutorIdAsync(tutor.UserID)).Count();

                var enrichedTutor = new EnrichedTutorDto
                {
                    UserID = tutor.UserID,
                    FullName = tutor.FullName,
                    Email = tutor.Email,
                    AvatarUrl = tutor.AvatarUrl,
                    Bio = userBio?.Bio ?? string.Empty,
                    Experience = userBio?.Experience ?? string.Empty,
                    HourlyRate = userBio?.HourlyRate ?? 0,
                    Availability = userBio?.Availability ?? string.Empty,
                    School = tutor.School,
                    AverageRating = averageRating,
                    ReviewCount = reviewCount,
                    Skills = skills ?? Enumerable.Empty<UserSkillDto>()
                };

                return Result<EnrichedTutorDto>.Success(enrichedTutor);
            }
            catch (Exception ex)
            {
                return Result<EnrichedTutorDto>.Failure($"Failed to retrieve enriched tutor data: {ex.Message}");
            }
        }
    }
}