using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PeerTutoringSystem.Api.Controllers.Booking;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
    [TestFixture]
    public class BookingsControllerTests
    {
        private Mock<IBookingService> _mockBookingService;
        private Mock<ILogger<BookingsController>> _mockLogger;
        private BookingsController _controller;
        private Guid _userId = Guid.NewGuid();
        private Guid _bookingId = Guid.NewGuid();
        private Guid _tutorId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _mockBookingService = new Mock<IBookingService>();
            _mockLogger = new Mock<ILogger<BookingsController>>();
            _controller = new BookingsController(_mockBookingService.Object, _mockLogger.Object);

            // Setup basic user claims for testing
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Role, "Student")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Test]
        public async Task CreateBooking_ValidData_ReturnsOkResult()
        {
            // Arrange
            var createDto = new CreateBookingDto
            {
                TutorId = _tutorId,
                AvailabilityId = Guid.NewGuid(),
                Topic = "Test Topic"
            };

            var bookingResult = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Topic = "Test Topic"
            };

            _mockBookingService
                .Setup(s => s.CreateBookingAsync(_userId, createDto))
                .ReturnsAsync(bookingResult!);

            // Act
            var result = await _controller.CreateBooking(createDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var responseObj = okResult.Value;
            Assert.That(responseObj, Is.Not.Null);
            if (responseObj is null)
            {
                Assert.Fail("Response object is null.");
                return;
            }
            var bookingData = (BookingSessionDto?)responseObj.GetType().GetProperty("data")?.GetValue(responseObj);
            Assert.That(bookingData, Is.Not.Null);
            Assert.That(bookingData?.BookingId, Is.EqualTo(_bookingId));
        }

        [Test]
        public async Task CreateBooking_ServiceThrowsValidationException_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateBookingDto();
            var exceptionMessage = "Validation error message";

            _mockBookingService
                .Setup(s => s.CreateBookingAsync(_userId, createDto))
                .ThrowsAsync(new ValidationException(exceptionMessage));

            // Act
            var result = await _controller.CreateBooking(createDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult?.Value?.ToString(), Does.Contain(exceptionMessage));
        }

        [Test]
        public async Task GetStudentBookings_ReturnsOkResultWithBookings()
        {
            // Arrange
            var filterDto = new BookingFilterDto(); // Create a filter DTO
            var studentBookings = new List<BookingSessionDto>
            {
                new BookingSessionDto { BookingId = Guid.NewGuid(), StudentId = _userId },
                new BookingSessionDto { BookingId = Guid.NewGuid(), StudentId = _userId }
            };
            var serviceResult = (Bookings: (IEnumerable<BookingSessionDto>)studentBookings, TotalCount: studentBookings.Count);

            _mockBookingService
                .Setup(s => s.GetBookingsByStudentAsync(_userId, It.IsAny<BookingFilterDto>()))
                .ReturnsAsync(serviceResult!);

            // Act
            var result = await _controller.GetStudentBookings(filterDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var responseObj = okResult.Value;
            Assert.That(responseObj, Is.Not.Null);
            var bookings = (IEnumerable<BookingSessionDto>?)responseObj?.GetType().GetProperty("data")?.GetValue(responseObj);
            var totalCountValue = responseObj?.GetType().GetProperty("totalCount")?.GetValue(responseObj);
            Assert.That(totalCountValue, Is.Not.Null);
            var totalCount = (int)totalCountValue;
            Assert.That(bookings, Is.Not.Null);
            Assert.That(totalCount, Is.EqualTo(2));
        }

        [Test]
        public async Task GetBooking_ExistingIdAndAuthorizedUser_ReturnsOkResult()
        {
            // Arrange
            var booking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId
            };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking!);

            // Act
            var result = await _controller.GetBooking(_bookingId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var responseObj = okResult.Value;
            Assert.That(responseObj, Is.Not.Null);
            var bookingData = (BookingSessionDto?)responseObj?.GetType().GetProperty("data")?.GetValue(responseObj);
            Assert.That(bookingData, Is.Not.Null);
            Assert.That(bookingData?.BookingId, Is.EqualTo(_bookingId));
        }

        [Test]
        public async Task GetBooking_NonexistentId_ReturnsNotFound()
        {
            // Arrange
            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync((BookingSessionDto?)null);

            // Act
            var result = await _controller.GetBooking(_bookingId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetBooking_UnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var differentUserId = Guid.NewGuid();

            var booking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = differentUserId,  // Different student ID
                TutorId = Guid.NewGuid()      // Different tutor ID
            };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking!);

            // Act
            var result = await _controller.GetBooking(_bookingId);

            // Assert
            var forbidResult = result as ObjectResult;
            Assert.That(forbidResult, Is.Not.Null);
            Assert.That(forbidResult?.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task UpdateBookingStatus_ValidStatusByStudent_ReturnsOkResult()
        {
            // Arrange
            // Setup the current user as a student
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Role, "Student")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            var booking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Status = BookingStatus.Pending
            };

            var updateDto = new UpdateBookingStatusDto { Status = "Cancelled" };

            var updatedBooking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Status = BookingStatus.Cancelled
            };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking!);

            _mockBookingService
                .Setup(s => s.UpdateBookingStatusAsync(_bookingId, It.IsAny<UpdateBookingStatusDto>()))
                .ReturnsAsync(updatedBooking!);

            // Act
            var result = await _controller.UpdateBookingStatus(_bookingId, updateDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var responseObj = okResult.Value;
            Assert.That(responseObj, Is.Not.Null);
            var bookingData = (BookingSessionDto?)responseObj?.GetType().GetProperty("data")?.GetValue(responseObj);
            Assert.That(bookingData, Is.Not.Null);
            Assert.That(bookingData?.Status, Is.EqualTo(BookingStatus.Cancelled));
        }

        [Test]
        public async Task UpdateBookingStatus_StudentCancellingOtherStudentBooking_ReturnsForbidden()
        {
            // Arrange
            var differentStudentId = Guid.NewGuid();

            var booking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = differentStudentId,  // Different student ID
                TutorId = _tutorId,
                Status = BookingStatus.Pending
            };

            var updateDto = new UpdateBookingStatusDto { Status = "Cancelled" };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking!);

            // Act
            var result = await _controller.UpdateBookingStatus(_bookingId, updateDto);

            // Assert
            var forbidResult = result as ObjectResult;
            Assert.That(forbidResult, Is.Not.Null);
            Assert.That(forbidResult?.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task UpdateBookingStatus_TutorConfirmingBooking_ReturnsOkResult()
        {
            // Arrange
            // Setup the current user as a tutor
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, _tutorId.ToString()),
                new Claim(ClaimTypes.Role, "Tutor")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            var booking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Status = BookingStatus.Pending
            };

            var updateDto = new UpdateBookingStatusDto { Status = "Confirmed" };

            var updatedBooking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Status = BookingStatus.Confirmed
            };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking!);

            _mockBookingService
                .Setup(s => s.UpdateBookingStatusAsync(_bookingId, It.IsAny<UpdateBookingStatusDto>()))
                .ReturnsAsync(updatedBooking!);

            // Act
            var result = await _controller.UpdateBookingStatus(_bookingId, updateDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var responseObj = okResult.Value;
            Assert.That(responseObj, Is.Not.Null);
            var bookingData = (BookingSessionDto?)responseObj?.GetType().GetProperty("data")?.GetValue(responseObj);
            Assert.That(bookingData, Is.Not.Null);
            Assert.That(bookingData?.Status, Is.EqualTo(BookingStatus.Confirmed));
        }
    }
}