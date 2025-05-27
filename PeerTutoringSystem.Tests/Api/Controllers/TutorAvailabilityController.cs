using Microsoft.AspNetCore.Mvc;
using Moq;
using PeerTutoringSystem.Api.Controllers.Booking;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.Interfaces.Booking;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using NUnit.Framework;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
    [TestFixture]
    public class TutorAvailabilityControllerTests
    {
        private Mock<ITutorAvailabilityService> _mockService;
        private TutorAvailabilityController _controller;
        private Guid _tutorId;
        private Guid _availabilityId;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<ITutorAvailabilityService>();
            _controller = new TutorAvailabilityController(_mockService.Object);
            _tutorId = Guid.NewGuid();
            _availabilityId = Guid.NewGuid();

            // Set up a mock user identity (tutor role)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _tutorId.ToString()),
                new Claim(ClaimTypes.Role, "Tutor")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task AddAvailability_ValidData_ReturnsOkResult()
        {
            // Arrange
            var createDto = new CreateTutorAvailabilityDto
            {
                StartTime = DateTime.UtcNow.AddHours(24),
                EndTime = DateTime.UtcNow.AddHours(26),
                IsRecurring = false
            };

            var availabilityDto = new TutorAvailabilityDto
            {
                AvailabilityId = _availabilityId,
                TutorId = _tutorId,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                IsRecurring = false,
                IsBooked = false
            };

            _mockService
                .Setup(s => s.AddAsync(_tutorId, createDto))
                .ReturnsAsync(availabilityDto);

            // Act
            var result = await _controller.AddAvailability(createDto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            var returnValue = okResult.Value as TutorAvailabilityDto;
            Assert.That(returnValue, Is.Not.Null);
            Assert.That(returnValue.AvailabilityId, Is.EqualTo(_availabilityId));
            Assert.That(returnValue.TutorId, Is.EqualTo(_tutorId));
        }

        [Test]
        public async Task GetTutorAvailability_ExistingTutor_ReturnsOkWithAvailabilities()
        {
            // Arrange
            var availabilityList = new List<TutorAvailabilityDto>
            {
                new TutorAvailabilityDto
                {
                    AvailabilityId = _availabilityId,
                    TutorId = _tutorId,
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                    IsBooked = false
                },
                new TutorAvailabilityDto
                {
                    AvailabilityId = Guid.NewGuid(),
                    TutorId = _tutorId,
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddHours(2),
                    IsBooked = false
                }
            };

            _mockService
                .Setup(s => s.GetByTutorIdAsync(_tutorId))
                .ReturnsAsync(availabilityList);

            // Act
            var result = await _controller.GetTutorAvailability(_tutorId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            var returnValue = okResult.Value as IEnumerable<TutorAvailabilityDto>;
            Assert.That(returnValue, Is.Not.Null);
            Assert.That(returnValue.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAvailableSlots_ValidDateRange_ReturnsOkWithFilteredSlots()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(7);

            var availabilityList = new List<TutorAvailabilityDto>
            {
                new TutorAvailabilityDto
                {
                    AvailabilityId = _availabilityId,
                    TutorId = _tutorId,
                    StartTime = startDate.AddDays(1).AddHours(9),
                    EndTime = startDate.AddDays(1).AddHours(11),
                    IsBooked = false
                },
                new TutorAvailabilityDto
                {
                    AvailabilityId = Guid.NewGuid(),
                    TutorId = _tutorId,
                    StartTime = startDate.AddDays(2).AddHours(10),
                    EndTime = startDate.AddDays(2).AddHours(12),
                    IsBooked = false
                }
            };

            _mockService
                .Setup(s => s.GetAvailableSlotsAsync(_tutorId, startDate, endDate))
                .ReturnsAsync(availabilityList);

            // Act
            var result = await _controller.GetAvailableSlots(_tutorId, startDate, endDate);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            var returnValue = okResult.Value as IEnumerable<TutorAvailabilityDto>;
            Assert.That(returnValue, Is.Not.Null);
            Assert.That(returnValue.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteAvailability_OwnAvailability_ReturnsOkResult()
        {
            // Arrange
            var availability = new TutorAvailabilityDto
            {
                AvailabilityId = _availabilityId,
                TutorId = _tutorId,  // Same as the user's ID
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            _mockService
                .Setup(s => s.GetByIdAsync(_availabilityId))
                .ReturnsAsync(availability);

            _mockService
                .Setup(s => s.DeleteAsync(_availabilityId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAvailability(_availabilityId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            var message = okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value, null) as string;
            Assert.That(message, Is.Not.Null);
            Assert.That(message, Does.Contain("successfully"));
        }

        [Test]
        public async Task DeleteAvailability_AnotherTutorsAvailability_ReturnsForbidden()
        {
            // Arrange
            var anotherTutorId = Guid.NewGuid();

            var availability = new TutorAvailabilityDto
            {
                AvailabilityId = _availabilityId,
                TutorId = anotherTutorId,  // Different tutor ID
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            _mockService
                .Setup(s => s.GetByIdAsync(_availabilityId))
                .ReturnsAsync(availability);

            // Act
            var result = await _controller.DeleteAvailability(_availabilityId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var forbidResult = result as ObjectResult;
            Assert.That(forbidResult, Is.Not.Null);
            Assert.That(forbidResult.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task DeleteAvailability_NonexistentAvailability_ReturnsNotFound()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid();

            _mockService
                .Setup(s => s.GetByIdAsync(nonexistentId))
                .ReturnsAsync((TutorAvailabilityDto)null);

            // Act
            var result = await _controller.DeleteAvailability(nonexistentId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }
    }
}