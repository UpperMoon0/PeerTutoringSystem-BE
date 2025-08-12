using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PayOSWebhookService : IPayOSWebhookService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly IUserBioRepository _userBioRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PayOSWebhookService> _logger;

        public PayOSWebhookService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IBookingSessionRepository bookingRepository, IUserBioRepository userBioRepository, ISessionRepository sessionRepository, IUserRepository userRepository, IPaymentRepository paymentRepository, ILogger<PayOSWebhookService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _bookingRepository = bookingRepository;
            _userBioRepository = userBioRepository;
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<string> ConfirmWebhook()
        {
            var clientId = Environment.GetEnvironmentVariable("PayOS_Client_ID");
            var apiKey = Environment.GetEnvironmentVariable("PayOS_API_Key");
            var webhookUrl = "https://peertutoringsystem-be.onrender.com/api/Payment/payos-webhook";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey))
            {
                var errorMessage = "PayOS ClientId or ApiKey is not configured in .env file.";
                _logger.LogError(errorMessage);
                return errorMessage;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            _logger.LogInformation($"PayOS webhook headers: x-client-id={clientId}, x-api-key={apiKey}");

            var requestBody = new { webhookUrl };
            var jsonPayload = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation($"PayOS webhook request payload: {jsonPayload}");
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api-merchant.payos.vn/confirm-webhook", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully confirmed PayOS webhook.");
                    return $"Successfully confirmed PayOS webhook. Response: {responseBody}";
                }
                else
                {
                    _logger.LogError($"Failed to confirm PayOS webhook. Status: {response.StatusCode}, Body: {responseBody}");
                    return $"Failed to confirm PayOS webhook. Status: {response.StatusCode}, Body: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while confirming the PayOS webhook.");
                return $"An error occurred while confirming the PayOS webhook: {ex.Message}";
            }
        }

        public async Task ProcessPayOSWebhook(PayOSWebhookData webhookData)
        {
            var checksumKey = Environment.GetEnvironmentVariable("PayOS_Checksum_Key");
            if (string.IsNullOrEmpty(checksumKey))
            {
                throw new Exception("PayOS Checksum Key is not configured in .env file.");
            }

            var dataDict = new Dictionary<string, string>();
            var properties = webhookData.Data.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var jsonPropertyName = prop.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                                            .OfType<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                                            .FirstOrDefault();
                var key = jsonPropertyName != null ? jsonPropertyName.Name : prop.Name;
                var value = prop.GetValue(webhookData.Data)?.ToString();
                if (value != null)
                {
                    dataDict.Add(key, value);
                }
            }
            var signature = GenerateSignature(dataDict, checksumKey);

            if (signature != webhookData.Signature)
            {
                _logger.LogError("Signature mismatch. Received: {receivedSignature}, Generated: {generatedSignature}", webhookData.Signature, signature);
                throw new Exception("Invalid signature");
            }

            if (webhookData.Code == "00")
            {
                var booking = await _bookingRepository.GetByOrderCode(webhookData.Data.OrderCode);
                if (booking != null)
                {
                    booking.PaymentStatus = PaymentStatus.Paid;
                    await _bookingRepository.UpdateAsync(booking);

                    var payment = new PaymentEntity
                    {
                        BookingId = booking.BookingId,
                        Amount = booking.basePrice + booking.serviceFee,
                        Status = PaymentStatus.Paid,
                        TransactionId = webhookData.Data.Reference,
                        Description = webhookData.Data.Description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _paymentRepository.AddAsync(payment);

                    var session = await _sessionRepository.GetByBookingIdAsync(booking.BookingId);
                    if (session != null)
                    {
                        var tutor = await _userRepository.GetByIdAsync(booking.TutorId);
                        if (tutor != null)
                        {
                            var tutorBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
                            if (tutorBio != null)
                            {
                                var duration = (decimal)(booking.EndTime - booking.StartTime).TotalHours;
                                var amountToPay = duration * tutorBio.HourlyRate;
                                tutor.AccountBalance += (double)amountToPay;
                                await _userRepository.UpdateAsync(tutor);
                            }
                        }
                    }
                }
            }
        }

        private string CreateSignature(string data, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        private string GenerateSignature(Dictionary<string, string> data, string key)
        {
            var sortedData = new SortedDictionary<string, string>(data);
            var dataToSign = string.Join("&", sortedData.Select(kv => $"{kv.Key}={kv.Value}"));
            var signature = CreateSignature(dataToSign, key);
            _logger.LogInformation("Data to sign: {dataToSign}", dataToSign);
            _logger.LogInformation("Generated Signature: {signature}", signature);
            return signature;
        }

        public async Task<string> ProcessWebhook(HttpRequest request)
        {
            using var reader = new System.IO.StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("Received webhook: {body}", body);

            try
            {
                var webhookData = JsonSerializer.Deserialize<PayOSWebhookData>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData != null)
                {
                    await ProcessPayOSWebhook(webhookData);
                    return "Webhook processed successfully.";
                }
                else
                {
                    _logger.LogWarning("Webhook data is null after deserialization.");
                    return "Webhook data is null.";
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing webhook JSON.");
                return "Error deserializing webhook JSON.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook.");
                return "Error processing webhook.";
            }
        }
    }
}