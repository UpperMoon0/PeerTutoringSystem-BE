using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PayOSService : IPayOSService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly IUserBioRepository _userBioRepository;
        private readonly ISessionRepository _sessionRepository;

        public PayOSService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IBookingSessionRepository bookingRepository,
            IUserBioRepository userBioRepository,
            ISessionRepository sessionRepository)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _bookingRepository = bookingRepository;
            _userBioRepository = userBioRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<PayOSCreatePaymentLinkResponseDto> CreatePaymentLink(PayOSCreatePaymentLinkRequestDto request, string successUrl, string cancelUrl)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            var tutorBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
            if (tutorBio == null)
            {
                throw new Exception("Tutor not found");
            }

            var session = await _sessionRepository.GetByBookingIdAsync(request.BookingId);
            if (session == null)
            {
                throw new Exception("Session not found");
            }

            var durationHours = (session.EndTime - session.StartTime).TotalHours;
            var basePrice = (decimal)durationHours * tutorBio.HourlyRate;
            var serviceFee = basePrice * 0.3m;
            var amount = (int)(basePrice + serviceFee);
            var description = $"Booking {request.BookingId}";
            if (description.Length > 25)
            {
                description = description.Substring(0, 25);
            }
            var orderCode = new Random().Next(100000, 999999);

            var items = new List<PayOSItemDto>
            {
                new PayOSItemDto
                {
                    name = $"Tutoring session with {tutorBio.User.FullName}",
                    quantity = 1,
                    price = amount
                }
            };

            var checksumKey = _configuration["PayOS_Checksum_Key"];
            if (string.IsNullOrEmpty(checksumKey))
            {
                throw new Exception("PayOS Checksum Key is not configured.");
            }
            
            var dataToSign = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={successUrl}";
            var signature = CreateSignature(dataToSign, checksumKey);

            var payOSRequest = new
            {
                orderCode,
                amount,
                description,
                cancelUrl,
                returnUrl = successUrl,
                items,
                signature
            };

            var client = _httpClientFactory.CreateClient();
            var clientId = _configuration["PayOS_Client_ID"];
            var apiKey = _configuration["PayOS_API_Key"];

            var jsonRequest = JsonSerializer.Serialize(payOSRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);

            if (response == null)
            {
                throw new Exception("Failed to get response from PayOS");
            }
            
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PayOSCreatePaymentLinkResponseDto>(jsonResponse);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize PayOS response");
                }
                return result;
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create payment link: {errorResponse}");
        }

        private string CreateSignature(string data, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}