using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using PeerTutoringSystem.Api.Controllers.Booking;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
    [TestClass]
    public class BookingsControllerTests
    {
        private Mock<IBookingService> _mockBookingService;
        private BookingsController _controller;
        private Guid _userId = Guid.NewGuid();
        private Guid _bookingId = Guid.NewGuid();
        private Guid _tutorId = Guid.NewGuid();

        [TestInitialize]
        public void Setup()
        {
            _mockBookingService = new Mock<IBookingService>();
            _controller = new BookingsController(_mockBookingService.Object);

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

        [TestMethod]
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
                .ReturnsAsync(bookingResult);

            // Act
            var result = await _controller.CreateBooking(createDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnValue = okResult.Value as BookingSessionDto;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(_bookingId, returnValue.BookingId);
        }

        [TestMethod]
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
            Assert.IsNotNull(badRequestResult);
            Assert.IsTrue(badRequestResult.Value.ToString().Contains(exceptionMessage));
        }

        [TestMethod]
        public async Task GetStudentBookings_ReturnsOkResultWithBookings()
        {
            // Arrange
            var bookings = new List<BookingSessionDto>
            {
                new BookingSessionDto { BookingId = Guid.NewGuid(), StudentId = _userId },
                new BookingSessionDto { BookingId = Guid.NewGuid(), StudentId = _userId }
            };

            _mockBookingService
                .Setup(s => s.GetBookingsByStudentAsync(_userId))
                .ReturnsAsync(bookings);

            // Act
            var result = await _controller.GetStudentBookings();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnValue = okResult.Value as IEnumerable<BookingSessionDto>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, ((List<BookingSessionDto>)returnValue).Count);
        }

        [TestMethod]
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
                .ReturnsAsync(booking);

            // Act
            var result = await _controller.GetBooking(_bookingId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnValue = okResult.Value as BookingSessionDto;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(_bookingId, returnValue.BookingId);
        }

        [TestMethod]
        public async Task GetBooking_NonexistentId_ReturnsNotFound()
        {
            // Arrange
            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync((BookingSessionDto)null);

            // Act
            var result = await _controller.GetBooking(_bookingId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
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
                .ReturnsAsync(booking);

            // Act
            var result = await _controller.GetBooking(_bookingId);

            // Assert
            var forbidResult = result as ObjectResult;
            Assert.IsNotNull(forbidResult);
            Assert.AreEqual(403, forbidResult.StatusCode);
        }

        [TestMethod]
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
                Status = "Pending"
            };

            var updateDto = new UpdateBookingStatusDto { Status = "Cancelled" };

            var updatedBooking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Status = "Cancelled"
            };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking);

            _mockBookingService
                .Setup(s => s.UpdateBookingStatusAsync(_bookingId, updateDto))
                .ReturnsAsync(updatedBooking);

            // Act
            var result = await _controller.UpdateBookingStatus(_bookingId, updateDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnValue = okResult.Value as BookingSessionDto;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual("Cancelled", returnValue.Status);
        }

        [TestMethod]
        public async Task UpdateBookingStatus_StudentCancellingOtherStudentBooking_ReturnsForbidden()
        {
            // Arrange
            var differentStudentId = Guid.NewGuid();

            var booking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = differentStudentId,  // Different student ID
                TutorId = _tutorId,
                Status = "Pending"
            };

            var updateDto = new UpdateBookingStatusDto { Status = "Cancelled" };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking);

            // Act
            var result = await _controller.UpdateBookingStatus(_bookingId, updateDto);

            // Assert
            var forbidResult = result as ObjectResult;
            Assert.IsNotNull(forbidResult);
            Assert.AreEqual(403, forbidResult.StatusCode);
        }

        [TestMethod]
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
                Status = "Pending"
            };

            var updateDto = new UpdateBookingStatusDto { Status = "Confirmed" };

            var updatedBooking = new BookingSessionDto
            {
                BookingId = _bookingId,
                StudentId = _userId,
                TutorId = _tutorId,
                Status = "Confirmed"
            };

            _mockBookingService
                .Setup(s => s.GetBookingByIdAsync(_bookingId))
                .ReturnsAsync(booking);

            _mockBookingService
                .Setup(s => s.UpdateBookingStatusAsync(_bookingId, updateDto))
                .ReturnsAsync(updatedBooking);

            // Act
            var result = await _controller.UpdateBookingStatus(_bookingId, updateDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnValue = okResult.Value as BookingSessionDto;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual("Confirmed", returnValue.Status);
        }
    }
}