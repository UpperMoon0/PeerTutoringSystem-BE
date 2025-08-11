using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Tests.Integration
{
    [TestFixture]
    [Ignore("Inconclusive due to testhost.deps.json issue")]
    public class BookingIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private string _studentJwt;
        private string _tutorJwt;
        private Guid _studentId;
        private Guid _tutorId;
        private Guid _availabilityId;
        private Guid _bookingId;

        [SetUp]
        public async Task Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Replace the real database with an in-memory one for testing
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("IntegrationTestDb");
                        });

                        // Build the service provider
                        var sp = services.BuildServiceProvider();

                        // Create scope and get DbContext
                        using var scope = sp.CreateScope();
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<AppDbContext>();

                        // Ensure database is created and add test data
                        db.Database.EnsureCreated();
                        InitializeTestData(db);
                    });
                    builder.UseSetting("APPLICATIONNAME", "PeerTutoringSystem.Api");
                });

            _client = _factory.CreateClient();

            // Get JWT tokens for test users
            _studentJwt = await GetJwtToken("student@example.com", "Password123!");
            _tutorJwt = await GetJwtToken("tutor@example.com", "Password123!");
        }

        private void InitializeTestData(AppDbContext db)
        {
            // Clear existing data
            db.Users.RemoveRange(db.Users);
            db.TutorAvailabilities.RemoveRange(db.TutorAvailabilities);
            db.BookingSessions.RemoveRange(db.BookingSessions);
            db.SaveChanges();

            // Add roles
            if (!db.Roles.Any())
            {
                db.Roles.AddRange(
                    new Role { RoleID = 1, RoleName = "Student" },
                    new Role { RoleID = 2, RoleName = "Tutor" },
                    new Role { RoleID = 3, RoleName = "Admin" }
                );
                db.SaveChanges();
            }

            // Add users
            _studentId = Guid.NewGuid();
            _tutorId = Guid.NewGuid();

            db.Users.AddRange(
                new User
                {
                    UserID = _studentId,
                    Email = "student@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    DateOfBirth = new DateTime(2000, 1, 1),
                    FullName = "Student",
                    PhoneNumber = "1234567890",
                    Gender = Gender.Male,
                    Hometown = "Test City",
                    CreatedDate = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    RoleID = 1
                },
                new User
                {
                    UserID = _tutorId,
                    Email = "tutor@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    DateOfBirth = new DateTime(1995, 1, 1),
                    FullName = "Tutor",
                    PhoneNumber = "0987654321",
                    Gender = Gender.Female,
                    Hometown = "Tutor City",
                    CreatedDate = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    RoleID = 2
                }
            );
            db.SaveChanges();

            // Add availability
            _availabilityId = Guid.NewGuid();
            db.TutorAvailabilities.Add(new TutorAvailability
            {
                AvailabilityId = _availabilityId,
                TutorId = _tutorId,
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3),
                IsRecurring = false,
                IsBooked = false
            });
            db.SaveChanges();

            // Add booking
            _bookingId = Guid.NewGuid();
            db.BookingSessions.Add(new BookingSession
            {
                BookingId = _bookingId,
                StudentId = _studentId,
                TutorId = _tutorId,
                AvailabilityId = _availabilityId,
                SessionDate = DateTime.UtcNow.Date,
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                Topic = "Test Booking",
                Description = "This is a test booking",
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        private async Task<string> GetJwtToken(string email, string password)
        {
            var loginData = new
            {
                Email = email,
                Password = password
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginData),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeAnonymousType(responseContent, new { token = "" });

            return tokenData?.token ?? string.Empty;
        }

        [Test]
        public async Task CreateBooking_WithValidData_ShouldReturn200()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _studentJwt);

            var newBookingData = new
            {
                TutorId = _tutorId,
                AvailabilityId = _availabilityId,
                Topic = "New Integration Test Booking",
                Description = "Testing the booking API integration"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(newBookingData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/bookings", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var bookingData = JsonConvert.DeserializeObject<BookingSessionDto>(responseContent);
            Assert.IsNotNull(bookingData);
            Assert.AreEqual(_studentId, bookingData.StudentId);
            Assert.AreEqual(_tutorId, bookingData.TutorId);
            Assert.AreEqual("New Integration Test Booking", bookingData.Topic);
        }

        [Test]
        public async Task GetStudentBookings_ShouldReturnListOfBookings()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _studentJwt);

            // Act
            var response = await _client.GetAsync("/api/bookings/student");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var bookingsData = JsonConvert.DeserializeObject<List<BookingSessionDto>>(responseContent);
            Assert.IsNotNull(bookingsData);
            Assert.IsTrue(bookingsData.Count > 0);
            Assert.AreEqual(_studentId, bookingsData[0].StudentId);
        }

        [Test]
        public async Task UpdateBookingStatus_TutorConfirmingBooking_ShouldReturnUpdatedBooking()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tutorJwt);

            var updateData = new
            {
                Status = "Confirmed"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/bookings/{_bookingId}/status", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var bookingData = JsonConvert.DeserializeObject<BookingSessionDto>(responseContent);
            Assert.IsNotNull(bookingData);
            Assert.AreEqual("Confirmed", bookingData.Status);
        }

        [Test]
        public async Task Unauthorized_ShouldReturn401()
        {
            // Arrange - no authentication headers

            // Act
            var response = await _client.GetAsync("/api/bookings/student");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task StudentAttemptingToConfirmBooking_ShouldReturn403()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _studentJwt);

            var updateData = new
            {
                Status = "Confirmed"  // Only tutors should be able to confirm bookings
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/bookings/{_bookingId}/status", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}