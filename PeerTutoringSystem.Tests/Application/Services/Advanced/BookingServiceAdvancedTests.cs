using Moq;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Authentication;
using PeerTutoringSystem.Application.Interfaces.Booking;
using PeerTutoringSystem.Application.Services.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using System.ComponentModel.DataAnnotations;
using PeerTutoringSystem.Domain.Interfaces.Skills;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using Microsoft.Extensions.Logging;
using PeerTutoringSystem.Domain.Interfaces.Authentication;

namespace PeerTutoringSystem.Tests.Application.Services.Advanced
{
    [TestFixture]
    public class BookingServiceAdvancedTests
    {
        [Test]
        public async Task CancelBooking_ShouldMakeSlotAvailableAgain()
        {
            // Arrange
            var fixture = new BookingServiceTestFixture();
            var bookingId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();

            var booking = new BookingSession
            {
                BookingId = bookingId,
                AvailabilityId = availabilityId,
                Status = BookingStatus.Pending
            };

            var availability = new TutorAvailability
            {
                AvailabilityId = availabilityId,
                IsBooked = true
            };

            fixture.MockBookingRepository
                .Setup(r => r.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);

            fixture.MockAvailabilityRepository
                .Setup(r => r.GetByIdAsync(availabilityId))
                .ReturnsAsync(availability);

            var updateDto = new UpdateBookingStatusDto { Status = "Cancelled" };

            // Act
            await fixture.BookingService.UpdateBookingStatusAsync(bookingId, updateDto);

            // Assert
            fixture.MockAvailabilityRepository.Verify(
                r => r.UpdateAsync(It.Is<TutorAvailability>(a => !a.IsBooked)),
                Times.Once);
        }

        [Test]
        public void CompleteBookingInFuture_ShouldThrowValidationException()
        {
            // Arrange
            var fixture = new BookingServiceTestFixture();
            var bookingId = Guid.NewGuid();

            var booking = new BookingSession
            {
                BookingId = bookingId,
                StartTime = DateTime.UtcNow.AddHours(1),  // Future time
                EndTime = DateTime.UtcNow.AddHours(2),    // Future time
                Status = BookingStatus.Confirmed
            };

            fixture.MockBookingRepository
                .Setup(r => r.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);

            var updateDto = new UpdateBookingStatusDto { Status = "Completed" };

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await fixture.BookingService.UpdateBookingStatusAsync(bookingId, updateDto));
        }

        [Test]
        public async Task GetUpcomingBookings_StudentRole_ReturnsOnlyStudentBookings()
        {
            // Arrange
            var fixture = new BookingServiceTestFixture();
            var studentId = Guid.NewGuid();

            var upcomingBookings = new List<BookingSession>
            {
                new BookingSession { StudentId = studentId, StartTime = DateTime.UtcNow.AddHours(1) }
            };

            fixture.MockBookingRepository
                .Setup(r => r.GetUpcomingBookingsByUserAsync(studentId, false)) // isTutor = false
                .ReturnsAsync(upcomingBookings);

            // Act
            var result = await fixture.BookingService.GetUpcomingBookingsAsync(studentId, false, new BookingFilterDto());

            // Assert
            fixture.MockBookingRepository.Verify(
                r => r.GetUpcomingBookingsByUserAsync(studentId, false),
                Times.Once);
        }

        [Test]
        public async Task GetUpcomingBookings_TutorRole_ReturnsOnlyTutorBookings()
        {
            // Arrange
            var fixture = new BookingServiceTestFixture();
            var tutorId = Guid.NewGuid();

            var upcomingBookings = new List<BookingSession>
            {
                new BookingSession { TutorId = tutorId, StartTime = DateTime.UtcNow.AddHours(2) }
            };

            fixture.MockBookingRepository
                .Setup(r => r.GetUpcomingBookingsByUserAsync(tutorId, true)) // isTutor = true
                .ReturnsAsync(upcomingBookings);

            // Act
            var result = await fixture.BookingService.GetUpcomingBookingsAsync(tutorId, true, new BookingFilterDto());

            // Assert
            fixture.MockBookingRepository.Verify(
                r => r.GetUpcomingBookingsByUserAsync(tutorId, true),
                Times.Once);
        }

        [Test]
        public async Task BookingWithNullTopic_ShouldUseDefaultTopic()
        {
            // Arrange
            var fixture = new BookingServiceTestFixture();
            var studentId = Guid.NewGuid();
            var tutorId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();

            var availability = new TutorAvailability
            {
                AvailabilityId = availabilityId,
                TutorId = tutorId,
                StartTime = DateTime.UtcNow.AddHours(3),
                EndTime = DateTime.UtcNow.AddHours(4),
                IsBooked = false
            };

            fixture.MockAvailabilityRepository
                .Setup(r => r.GetByIdAsync(availabilityId))
                .ReturnsAsync(availability);

            fixture.MockBookingRepository
                .Setup(r => r.IsSlotAvailableAsync(tutorId, availability.StartTime, availability.EndTime))
                .ReturnsAsync(true);

            var createBookingDto = new CreateBookingDto
            {
                TutorId = tutorId,
                AvailabilityId = availabilityId,
                Topic = null!,
                Description = "Testing null topic handling"
            };

            // Act
            var result = await fixture.BookingService.CreateBookingAsync(studentId, createBookingDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Topic, Is.EqualTo("General tutoring session"));
        }

        // Test fixture to reduce boilerplate code
        private class BookingServiceTestFixture
        {
            public Mock<IBookingSessionRepository> MockBookingRepository { get; }
            public Mock<ISessionRepository> MockSessionRepository { get; }
            public Mock<ITutorAvailabilityRepository> MockAvailabilityRepository { get; }
            public Mock<IUserService> MockUserService { get; }
            public Mock<ISkillRepository> MockSkillRepository { get; }
            public Mock<ITutorAvailabilityService> MockTutorAvailabilityService { get; }
            public Mock<IUserBioRepository> MockUserBioRepository { get; }
            public Mock<IUserRepository> MockUserRepository { get; }
            public Mock<ILogger<BookingService>> MockLogger { get; }
            public BookingService BookingService { get; }

            public BookingServiceTestFixture()
            {
                MockBookingRepository = new Mock<IBookingSessionRepository>();
                MockSessionRepository = new Mock<ISessionRepository>();
                MockAvailabilityRepository = new Mock<ITutorAvailabilityRepository>();
                MockUserService = new Mock<IUserService>();
                MockSkillRepository = new Mock<ISkillRepository>();
                MockTutorAvailabilityService = new Mock<ITutorAvailabilityService>();
                MockUserBioRepository = new Mock<IUserBioRepository>();
                MockUserRepository = new Mock<IUserRepository>();
                MockLogger = new Mock<ILogger<BookingService>>();

                BookingService = new BookingService(
                    MockBookingRepository.Object,
                    MockSessionRepository.Object,
                    MockAvailabilityRepository.Object,
                    MockTutorAvailabilityService.Object,
                    MockUserService.Object,
                    MockSkillRepository.Object,
                    MockUserBioRepository.Object,
                    MockUserRepository.Object,
                    MockLogger.Object);
            }
        }
    }
}