using PeerTutoringSystem.Application.DTOs;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PeerTutoringSystem.Application.Services.Booking
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserBioRepository _userBioRepository;

        public SessionService(
            ISessionRepository sessionRepository,
            IBookingSessionRepository bookingRepository,
            IHttpContextAccessor httpContextAccessor,
            IUserBioRepository userBioRepository)
        {
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userBioRepository = userBioRepository ?? throw new ArgumentNullException(nameof(userBioRepository));
        }

        public async Task<SessionDto> CreateSessionAsync(Guid userId, Guid bookingId, string videoCallLink, string sessionNotes, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ValidationException("Booking not found.");

            if (booking.TutorId != userId && booking.StudentId != userId)
                throw new ValidationException("You do not have permission to create a session for this booking.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new ValidationException("Only confirmed bookings can have a session created.");

            var tutorBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
            if (tutorBio == null)
            {
                throw new Exception("Tutor not found");
            }

            var durationHours = (endTime - startTime).TotalHours;
            var basePrice = (decimal)durationHours * tutorBio.HourlyRate;
            var serviceFee = basePrice * 0.3m;

            booking.basePrice = basePrice;
            booking.serviceFee = serviceFee;
            await _bookingRepository.UpdateAsync(booking);

            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                BookingId = bookingId,
                VideoCallLink = videoCallLink,
                SessionNotes = sessionNotes,
                StartTime = startTime.UtcDateTime,
                EndTime = endTime.UtcDateTime,
                CreatedAt = DateTime.UtcNow
            };

            var createdSession = await _sessionRepository.AddAsync(session);
            return MapToDto(createdSession);
        }

        public async Task<SessionDto> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return null;

            return MapToDto(session);
        }

        public async Task<SessionDto> GetSessionByBookingIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ValidationException("Booking not found.");

            var session = await _sessionRepository.GetByBookingIdAsync(bookingId);
            if (session == null)
                return null;

            return MapToDto(session);
        }

        public async Task<(IEnumerable<SessionDto> Sessions, int TotalCount)> GetSessionsByUserAsync(Guid userId, bool isTutor, BookingFilterDto filter)
        {
            var domainFilter = new BookingFilter(
                Page: filter.Page,
                PageSize: filter.PageSize,
                Status: filter.Status,
                SkillId: filter.SkillId,
                StartDate: filter.StartDate,
                EndDate: filter.EndDate
            );

            var sessions = await _sessionRepository.GetByUserIdAsync(userId, isTutor, domainFilter);
            var dtos = sessions.Sessions.Select(MapToDto);
            return (dtos, sessions.TotalCount);
        }

        public async Task<SessionDto> UpdateSessionAsync(Guid sessionId, string videoCallLink, string sessionNotes, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                throw new ValidationException("Session not found.");

            var userId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new ValidationException("Invalid user token."));

            var booking = await _bookingRepository.GetByIdAsync(session.BookingId);
            if (booking.TutorId != userId && booking.StudentId != userId)
                throw new ValidationException("You do not have permission to update this session.");

            session.VideoCallLink = videoCallLink ?? session.VideoCallLink;
            session.SessionNotes = sessionNotes ?? session.SessionNotes;
            session.StartTime = startTime.UtcDateTime;
            session.EndTime = endTime.UtcDateTime;
            session.UpdatedAt = DateTime.UtcNow;

            await _sessionRepository.UpdateAsync(session);
            return MapToDto(session);
        }

        private SessionDto MapToDto(Session session)
        {
            return new SessionDto
            {
                SessionId = session.SessionId,
                BookingId = session.BookingId,
                VideoCallLink = session.VideoCallLink,
                SessionNotes = session.SessionNotes,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };
        }
    }
}