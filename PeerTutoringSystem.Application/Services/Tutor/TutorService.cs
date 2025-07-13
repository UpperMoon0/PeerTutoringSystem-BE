using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Profile_Bio;
using PeerTutoringSystem.Application.DTOs.Skills;
using PeerTutoringSystem.Application.DTOs.Tutor;
using PeerTutoringSystem.Application.Helpers;
using PeerTutoringSystem.Application.Interfaces.Reviews;
using PeerTutoringSystem.Application.Interfaces.Skills;
using PeerTutoringSystem.Application.Interfaces.Tutor;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using System.Security.Claims;

namespace PeerTutoringSystem.Application.Services.Tutor
{
    public class TutorService : ITutorService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserBioRepository _userBioRepository;
        private readonly IUserSkillService _userSkillService;
        private readonly IReviewService _reviewService;
        private readonly IBookingSessionRepository _bookingSessionRepository;
        private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TutorService(
            IUserRepository userRepository,
            IUserBioRepository userBioRepository,
            IUserSkillService userSkillService,
            IReviewService reviewService,
            IBookingSessionRepository bookingSessionRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _userBioRepository = userBioRepository;
            _userSkillService = userSkillService;
            _reviewService = reviewService;
            _bookingSessionRepository = bookingSessionRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<Result<TutorDashboardStatsDto>> GetTutorDashboardStats()
        {
            try
            {
                var tutorIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (tutorIdClaim == null || !Guid.TryParse(tutorIdClaim.Value, out var tutorId))
                {
                    return Result<TutorDashboardStatsDto>.Failure("User not authenticated or invalid user ID.");
                }

                var tutor = await _userRepository.GetByIdAsync(tutorId);
                if (tutor == null || tutor.Role.RoleName != "Tutor")
                {
                    return Result<TutorDashboardStatsDto>.Failure("Tutor not found.");
                }

                var tutorBio = await _userBioRepository.GetByUserIdAsync(tutorId);
                var hourlyRate = tutorBio?.HourlyRate ?? 0;

                var bookings = await _bookingSessionRepository.GetByTutorIdAsync(tutorId);
                var totalBookings = bookings.Count();
                var completedSessions = bookings.Count(b => b.Status == BookingStatus.Completed);
                var totalEarnings = completedSessions * hourlyRate;

                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);
                var availableSlots = await _tutorAvailabilityRepository.GetAvailableSlotsByTutorIdAsync(tutorId, startOfWeek, endOfWeek);
                var availableSlotsCount = availableSlots.Count();

                var stats = new TutorDashboardStatsDto
                {
                    TotalBookings = totalBookings,
                    CompletedSessions = completedSessions,
                    TotalEarnings = (double)totalEarnings,
                    AvailableSlots = availableSlotsCount
                };

                return Result<TutorDashboardStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                return Result<TutorDashboardStatsDto>.Failure($"Failed to retrieve tutor dashboard stats: {ex.Message}");
            }
        }

        public async Task<Result<TutorFinanceDetailsDto>> GetTutorFinanceDetailsAsync()
        {
            try
            {
                var tutorIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (tutorIdClaim == null || !Guid.TryParse(tutorIdClaim.Value, out var tutorId))
                {
                    return Result<TutorFinanceDetailsDto>.Failure("User not authenticated or invalid user ID.");
                }

                var tutor = await _userRepository.GetByIdAsync(tutorId);
                if (tutor == null || tutor.Role.RoleName != "Tutor")
                {
                    return Result<TutorFinanceDetailsDto>.Failure("Tutor not found.");
                }

                var tutorBio = await _userBioRepository.GetByUserIdAsync(tutorId);
                var hourlyRate = tutorBio?.HourlyRate ?? 0;

                var bookings = await _bookingSessionRepository.GetByTutorIdAsync(tutorId);

                var now = DateTime.UtcNow;
                var firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
                var firstDayOfLastMonth = firstDayOfCurrentMonth.AddMonths(-1);
                var firstDayOfThisMonth = new DateTime(now.Year, now.Month, 1);

                var currentMonthEarnings = bookings
                    .Where(b => b.Status == BookingStatus.Completed && b.SessionDate >= firstDayOfThisMonth)
                    .Sum(b => hourlyRate);

                var lastMonthEarnings = bookings
                    .Where(b => b.Status == BookingStatus.Completed && b.SessionDate >= firstDayOfLastMonth && b.SessionDate < firstDayOfCurrentMonth)
                    .Sum(b => hourlyRate);

                var lifetimeEarnings = bookings
                    .Where(b => b.Status == BookingStatus.Completed)
                    .Sum(b => hourlyRate);

                var recentTransactions = bookings
                    .Where(b => b.Status == BookingStatus.Completed)
                    .OrderByDescending(b => b.SessionDate)
                    .Take(10)
                    .Select(b => new TransactionDto
                    {
                        Date = b.SessionDate,
                        Description = $"Session with student",
                        Amount = (double)hourlyRate
                    }).ToList();

                var earningsOverTime = new List<ChartDataPointDto>();
                for (int i = 5; i >= 0; i--)
                {
                    var month = now.AddMonths(-i);
                    var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    var monthlyEarnings = bookings
                        .Where(b => b.Status == BookingStatus.Completed && b.SessionDate >= firstDayOfMonth && b.SessionDate <= lastDayOfMonth)
                        .Sum(b => hourlyRate);

                    earningsOverTime.Add(new ChartDataPointDto
                    {
                        Label = month.ToString("MMM yyyy"),
                        Value = (double)monthlyEarnings
                    });
                }

                var financeDetails = new TutorFinanceDetailsDto
                {
                    RecentTransactions = recentTransactions,
                    EarningsOverTime = earningsOverTime,
                    CurrentMonthEarnings = (double)currentMonthEarnings,
                    LastMonthEarnings = (double)lastMonthEarnings,
                    LifetimeEarnings = (double)lifetimeEarnings
                };

                return Result<TutorFinanceDetailsDto>.Success(financeDetails);
            }
            catch (Exception ex)
            {
                return Result<TutorFinanceDetailsDto>.Failure($"Failed to retrieve tutor finance details: {ex.Message}");
            }
        }
    }
}