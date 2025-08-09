using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PeerTutoringSystem.Application.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly ITutorAvailabilityRepository _availabilityRepository;
        private readonly ITutorAvailabilityService _tutorAvailabilityService;
        private readonly IUserService _userService;
        private readonly ISkillRepository _skillRepository;
       private readonly Domain.Interfaces.Profile_Bio.IUserBioRepository _userBioRepository;
       private readonly ILogger<BookingService> _logger;

       public BookingService(
            IBookingSessionRepository bookingRepository,
            ITutorAvailabilityRepository availabilityRepository,
            ITutorAvailabilityService tutorAvailabilityService,
            IUserService userService,
            ISkillRepository skillRepository,
            Domain.Interfaces.Profile_Bio.IUserBioRepository userBioRepository,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _availabilityRepository = availabilityRepository ?? throw new ArgumentNullException(nameof(availabilityRepository));
            _tutorAvailabilityService = tutorAvailabilityService ?? throw new ArgumentNullException(nameof(tutorAvailabilityService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
            _userBioRepository = userBioRepository ?? throw new ArgumentNullException(nameof(userBioRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BookingSessionDto> CreateBookingAsync(Guid studentId, CreateBookingDto dto)
        {
            // Validate availability
            var availability = await _availabilityRepository.GetByIdAsync(dto.AvailabilityId);
            if (availability == null)
                throw new ValidationException("The selected time slot is not available.");

            if (availability.TutorId != dto.TutorId)
                throw new ValidationException("The selected time slot does not belong to the specified tutor.");

            if (availability.IsBooked)
                throw new ValidationException("This time slot has already been booked.");

            if (!await _bookingRepository.IsSlotAvailableAsync(dto.TutorId, availability.StartTime, availability.EndTime))
                throw new ValidationException("This time slot is no longer available.");

            // Validate skill if provided
            if (dto.SkillId.HasValue)
            {
                try {
                    var skill = await _skillRepository.GetByIdAsync(dto.SkillId.Value);
                    if (skill == null)
                        throw new ValidationException("The specified skill does not exist.");
                }
                catch (Exception) {
                    // If skill repository throws an exception, we'll default to null skillId
                    dto.SkillId = null;
                }
            }

            // Create booking
            var booking = new BookingSession
            {
                BookingId = Guid.NewGuid(),
                StudentId = studentId,
                TutorId = dto.TutorId,
                AvailabilityId = dto.AvailabilityId,
                SessionDate = availability.StartTime.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                SkillId = dto.SkillId,
                Topic = dto.Topic ?? "General tutoring session",
                Description = dto.Description,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };

            // Save booking and update availability
            await _bookingRepository.AddAsync(booking);

            availability.IsBooked = true;
            await _availabilityRepository.UpdateAsync(availability);

            // Return enriched booking
            return await EnrichBookingWithNames(booking);
        }

        public async Task<BookingSessionDto> CreateInstantBookingAsync(Guid studentId, InstantBookingDto dto)
        {
            var currentDateTimeUtc = DateTime.UtcNow;

            if (dto.StartTime < currentDateTimeUtc)
                throw new ValidationException("Start time cannot be in the past.");
            if (dto.EndTime <= dto.StartTime)
                throw new ValidationException("End time must be after start time.");
            if (dto.EndTime.Subtract(dto.StartTime).TotalMinutes < 30)
                throw new ValidationException("Session must be at least 30 minutes long.");

            if (!await _bookingRepository.IsSlotAvailableAsync(dto.TutorId, dto.StartTime, dto.EndTime))
                throw new ValidationException("This time slot is not available.");

            if (dto.SkillId.HasValue)
            {
                var skill = await _skillRepository.GetByIdAsync(dto.SkillId.Value);
                if (skill == null)
                    throw new ValidationException("The specified skill does not exist.");
            }

            var availability = new TutorAvailability
            {
                AvailabilityId = Guid.NewGuid(),
                TutorId = dto.TutorId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsRecurring = false,
                IsDailyRecurring = false,
                IsBooked = true
            };
            await _availabilityRepository.AddAsync(availability);

            var booking = new BookingSession
            {
                BookingId = Guid.NewGuid(),
                StudentId = studentId,
                TutorId = dto.TutorId,
                AvailabilityId = availability.AvailabilityId,
                SessionDate = dto.StartTime.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SkillId = dto.SkillId,
                Topic = dto.Topic ?? "General tutoring session",
                Description = dto.Description,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);
            return await EnrichBookingWithNames(booking);
        }

        public async Task<BookingSessionDto> GetBookingByIdAsync(Guid bookingId)
        {
            _logger.LogInformation("GetBookingByIdAsync called with bookingId: {BookingId}", bookingId);
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found.", bookingId);
                return null;
            }
            _logger.LogInformation("Retrieved booking: {Booking}", JsonSerializer.Serialize(booking));

            return await EnrichBookingWithNames(booking);
        }

        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetBookingsByStudentAsync(Guid studentId, BookingFilterDto filter)
        {
            var (bookings, totalCount) = await _bookingRepository.GetByStudentIdAsync(studentId, new Domain.Entities.Booking.BookingFilter
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                Status = filter.Status,
                SkillId = filter.SkillId,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate
            });

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetBookingsByTutorAsync(Guid tutorId, BookingFilterDto filter)
        {
            var (bookings, totalCount) = await _bookingRepository.GetByTutorIdAsync(tutorId, new Domain.Entities.Booking.BookingFilter
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                Status = filter.Status,
                SkillId = filter.SkillId,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate
            });

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetUpcomingBookingsAsync(Guid userId, bool isTutor, BookingFilterDto filter)
        {
            var query = await _bookingRepository.GetUpcomingBookingsByUserAsync(userId, isTutor);
            query = ApplyFilters(query, filter);

            var totalCount = query.Count();
            var bookings = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<BookingSessionDto> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ValidationException("Booking not found.");

            if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<BookingStatus>(dto.Status, true, out var newStatus))
            {
                if (IsValidStatusTransition(booking.Status, newStatus))
                {
                    booking.Status = newStatus;
                }
                else
                {
                    throw new ValidationException($"Invalid status transition from {booking.Status} to {newStatus}.");
                }
            }

            // Update availability status for Cancelled or Rejected
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected)
            {
                var availability = await _availabilityRepository.GetByIdAsync(booking.AvailabilityId);
                if (availability != null)
                {
                    availability.IsBooked = false;
                    await _availabilityRepository.UpdateAsync(availability);
                }
            }

            if (booking.Status == BookingStatus.Completed)
            {
                if (booking.EndTime > DateTime.UtcNow)
                    throw new ValidationException("Cannot mark a future booking as completed.");
            }

            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);

            return await EnrichBookingWithNames(booking);
        }

        private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
        {
            switch (currentStatus)
            {
                case BookingStatus.Pending:
                    return newStatus == BookingStatus.Confirmed || newStatus == BookingStatus.Cancelled || newStatus == BookingStatus.Rejected;
                case BookingStatus.Confirmed:
                    return newStatus == BookingStatus.Completed || newStatus == BookingStatus.Cancelled;
                case BookingStatus.Cancelled:
                case BookingStatus.Completed:
                case BookingStatus.Rejected:
                    return false; // No transitions allowed from Cancelled, Completed, or Rejected
                default:
                    return false;
            }
        }

        private IEnumerable<BookingSession> ApplyFilters(IEnumerable<BookingSession> query, BookingFilterDto filter)
        {
            // Apply status filter only if Status is provided and valid
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (Enum.TryParse<BookingStatus>(filter.Status, true, out var status))
                {
                    query = query.Where(b => b.Status == status);
                }
                // Ignore invalid Status values to return all bookings
            }

            if (filter.SkillId.HasValue)
            {
                query = query.Where(b => b.SkillId == filter.SkillId.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(b => b.StartTime >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(b => b.EndTime <= filter.EndDate.Value);
            }

            return query.OrderByDescending(b => b.StartTime);
        }

        private async Task<BookingSessionDto> EnrichBookingWithNames(BookingSession booking)
        {
            var studentName = "Unknown Student";
            var tutorName = "Unknown Tutor";
            decimal? price = null;

            try
            {
                var student = await _userService.GetUserByIdAsync(booking.StudentId);
                if (student != null)
                {
                    studentName = $"{student.FullName}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student details for student ID {StudentId}", booking.StudentId);
            }

            try
            {
                var tutor = await _userService.GetUserByIdAsync(booking.TutorId);
                if (tutor != null)
                {
                    tutorName = $"{tutor.FullName}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tutor details for tutor ID {TutorId}", booking.TutorId);
            }

            try
            {
                _logger.LogInformation("Fetching tutor bio for TutorId: {TutorId}", booking.TutorId);
                var userBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
                _logger.LogInformation("Retrieved tutor bio: {UserBio}", JsonSerializer.Serialize(userBio));

                if (userBio != null)
                {
                    _logger.LogInformation("Tutor HourlyRate: {HourlyRate}", userBio.HourlyRate);
                    _logger.LogInformation("Booking StartTime: {StartTime}, EndTime: {EndTime}", booking.StartTime, booking.EndTime);
                    var duration = booking.EndTime - booking.StartTime;
                    var totalHours = duration.TotalHours;
                    _logger.LogInformation("Calculated TotalHours: {TotalHours}", totalHours);
                    price = (decimal)totalHours * userBio.HourlyRate;
                    _logger.LogInformation("Calculated price: {Price}", price);
                }
                else
                {
                    _logger.LogWarning("Tutor bio not found for TutorId: {TutorId}", booking.TutorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for booking ID {BookingId}", booking.BookingId);
            }

            var bookingDto = new BookingSessionDto
            {
                BookingId = booking.BookingId,
                StudentId = booking.StudentId,
                TutorId = booking.TutorId,
                SessionDate = booking.SessionDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                SkillId = booking.SkillId,
                Topic = booking.Topic,
                Description = booking.Description,
                Status = booking.Status.ToString(),
                PaymentStatus = booking.PaymentStatus.ToString(),
                CreatedAt = booking.CreatedAt,
                StudentName = studentName,
                TutorName = tutorName,
                Price = price
            };

            _logger.LogInformation("Returning booking DTO: {BookingDto}", JsonSerializer.Serialize(bookingDto));
            return bookingDto;
        }
        public async Task<(IEnumerable<BookingSessionDto> Bookings, int TotalCount)> GetAllBookingsForAdminAsync(BookingFilterDto filter)
        {
            var query = await _bookingRepository.GetAllAsync(); 
            query = ApplyFilters(query, filter);

            var totalCount = query.Count();
            var bookings = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = new List<BookingSessionDto>();
            foreach (var booking in bookings)
            {
                dtos.Add(await EnrichBookingWithNames(booking));
            }

            return (dtos, totalCount);
        }

        public async Task<TutorDashboardStatsDto> GetTutorDashboardStatsAsync(Guid tutorId)
        {
            var allBookings = await _bookingRepository.GetByTutorIdAsync(tutorId);
            var allAvailabilities = await _tutorAvailabilityService.GetByTutorIdAsync(tutorId, new BookingFilterDto { PageSize = int.MaxValue });

            var totalBookings = allBookings.Count();
            var completedSessions = allBookings.Count(b => b.Status == BookingStatus.Completed);
            var pendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending);
            var confirmedBookings = allBookings.Count(b => b.Status == BookingStatus.Confirmed);
            
            var availableSlots = allAvailabilities.Availabilities.Count(a => !a.IsBooked && a.StartTime > DateTime.UtcNow);
            
            var totalEarnings = completedSessions * 50m;

            return new TutorDashboardStatsDto
            {
                TotalBookings = totalBookings,
                AvailableSlots = availableSlots,
                CompletedSessions = completedSessions,
                TotalEarnings = totalEarnings,
                PendingBookings = pendingBookings,
                ConfirmedBookings = confirmedBookings
            };
        }

        public async Task<UploadProofOfPaymentResult> UploadProofOfPayment(Guid bookingId, IFormFile file)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return new UploadProofOfPaymentResult { Succeeded = false, Message = "Booking not found." };
            }

            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "proofs");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            booking.ProofOfPaymentImageUrl = $"/proofs/{fileName}";
            await _bookingRepository.UpdateAsync(booking);

            return new UploadProofOfPaymentResult { Succeeded = true, FilePath = booking.ProofOfPaymentImageUrl };
        }

        public async Task<ConfirmPaymentResult> ConfirmPayment(Guid bookingId, PaymentConfirmationDto paymentConfirmationDto)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return new ConfirmPaymentResult { Succeeded = false, Message = "Booking not found." };
            }

            booking.PaymentStatus = Enum.Parse<PaymentStatus>(paymentConfirmationDto.Status, true);
            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                var tutor = await _userService.GetUserByIdAsync(booking.TutorId);
                var userBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
                if (tutor != null && userBio != null)
                {
                    var duration = booking.EndTime - booking.StartTime;
                    var totalHours = duration.TotalHours;
                    var price = (decimal)totalHours * userBio.HourlyRate;
                    tutor.AccountBalance += (double)price;
                    await _userService.UpdateUserAsync(tutor);
                }
            }

            await _bookingRepository.UpdateAsync(booking);

            return new ConfirmPaymentResult { Succeeded = true };
        }
    }

    public class UploadProofOfPaymentResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
    }

    public class ConfirmPaymentResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
    }
}