using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.Helpers;
using PeerTutoringSystem.Application.Interfaces.Reviews;
using PeerTutoringSystem.Application.Interfaces.Skills;
using PeerTutoringSystem.Application.Interfaces.Tutor;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Result<IEnumerable<EnrichedTutorDto>>> GetAllEnrichedTutorsAsync()
        {
            try
            {
                var tutors = await _userRepository.GetUsersByRoleAsync("Tutor");
                if (tutors == null || !tutors.Any())
                {
                    return Result<IEnumerable<EnrichedTutorDto>>.Success(Enumerable.Empty<EnrichedTutorDto>());
                }

                var enrichedTutors = new List<EnrichedTutorDto>();
                var tasks = tutors.Select(async tutor =>
                {
                    var userBio = await _userBioRepository.GetByUserIdAsync(tutor.UserID);
                    var skills = await _userSkillService.GetByUserIdAsync(tutor.UserID);
                    var averageRating = await _reviewService.GetAverageRatingByTutorIdAsync(tutor.UserID);
                    var reviewCount = (await _reviewService.GetReviewsByTutorIdAsync(tutor.UserID)).Count();

                    return new EnrichedTutorDto
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
                }).ToList();

                enrichedTutors.AddRange(await Task.WhenAll(tasks));

                return Result<IEnumerable<EnrichedTutorDto>>.Success(enrichedTutors);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<EnrichedTutorDto>>.Failure($"Failed to retrieve enriched tutor data: {ex.Message}");
            }
        }
    }
}