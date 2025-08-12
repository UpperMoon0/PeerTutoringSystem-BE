using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Payment;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IBookingSessionRepository bookingRepository,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistory(string userId)
        {
            var paymentHistory = await _paymentRepository.GetPaymentHistory(userId);
            return paymentHistory.Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Date = p.CreatedAt,
                Status = p.Status.ToString(),
                TutorName = p.Booking.Tutor.FullName,
                StudentName = p.Booking.Student.FullName
            });
        }

        public async Task<PayOSCreatePaymentLinkResponseDto> CreatePaymentLink(PayOSCreatePaymentLinkRequestDto request, string successUrl, string cancelUrl)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            var amount = (int)(booking.basePrice + booking.serviceFee);
            var description = $"Booking {request.BookingId}";
            if (description.Length > 25)
            {
                description = description.Substring(0, 25);
            }
            var orderCode = new Random().Next(100000, 999999);

            booking.OrderCode = orderCode;
            await _bookingRepository.UpdateAsync(booking);

            var items = new List<PayOSItemDto>
            {
                new PayOSItemDto
                {
                    name = $"Tutoring session with {booking.Tutor.FullName}",
                    quantity = 1,
                    price = amount
                }
            };

            var checksumKey = Environment.GetEnvironmentVariable("PayOS_Checksum_Key");
            if (string.IsNullOrEmpty(checksumKey))
            {
                throw new Exception("PayOS Checksum Key is not configured in .env file.");
            }
            
            var signatureData = new Dictionary<string, string>
            {
                { "amount", amount.ToString() },
                { "cancelUrl", cancelUrl },
                { "description", description },
                { "orderCode", orderCode.ToString() },
                { "returnUrl", successUrl }
            };
            var signature = GenerateSignature(signatureData, checksumKey);

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
            var clientId = Environment.GetEnvironmentVariable("PayOS_Client_ID");
            var apiKey = Environment.GetEnvironmentVariable("PayOS_API_Key");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("PayOS ClientId or ApiKey is not configured in .env file.");
            }

            var jsonRequest = JsonSerializer.Serialize(payOSRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var apiUrl = _configuration["PayOS:ApiUrl"];
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new Exception("PayOS API URL is not configured.");
            }
            var response = await client.PostAsync(apiUrl, content);

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
        private string GenerateSignature(Dictionary<string, string> data, string key)
        {
            var sortedData = new SortedDictionary<string, string>(data);
            var dataToSign = string.Join("&", sortedData.Select(kv => $"{kv.Key}={kv.Value}"));
            var signature = CreateSignature(dataToSign, key);
            _logger.LogInformation("Data to sign: {dataToSign}", dataToSign);
            _logger.LogInformation("Generated Signature: {signature}", signature);
            return signature;
        }

        public async Task<bool> ConfirmPayment(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            return booking != null && booking.PaymentStatus == PaymentStatus.Paid;
        }

        public async Task HandlePayOSWebhook(PayOSWebhookData webhookData)
        {
            var checksumKey = Environment.GetEnvironmentVariable("PayOS_Checksum_Key");
            if (string.IsNullOrEmpty(checksumKey))
            {
                throw new Exception("PayOS Checksum Key is not configured in .env file.");
            }

            var signatureData = new Dictionary<string, string>
            {
                { "orderCode", webhookData.Data.OrderCode.ToString() },
                { "amount", webhookData.Data.Amount.ToString() },
                { "description", webhookData.Data.Description },
                { "accountNumber", webhookData.Data.AccountNumber },
                { "reference", webhookData.Data.Reference },
                { "transactionDateTime", webhookData.Data.TransactionDateTime },
                { "paymentLinkId", webhookData.Data.PaymentLinkId },
                { "code", webhookData.Data.Code },
                { "desc", webhookData.Data.Desc },
                { "currency", webhookData.Data.Currency },
                { "counterAccountBankId", webhookData.Data.CounterAccountBankId },
                { "counterAccountBankName", webhookData.Data.CounterAccountBankName },
                { "counterAccountName", webhookData.Data.CounterAccountName },
                { "counterAccountNumber", webhookData.Data.CounterAccountNumber },
                { "virtualAccountName", webhookData.Data.VirtualAccountName },
                { "virtualAccountNumber", webhookData.Data.VirtualAccountNumber }
            };
            var signature = GenerateSignature(signatureData, checksumKey);

            if (signature != webhookData.Signature)
            {
                throw new Exception("Invalid signature");
            }

            if (webhookData.Code == "00")
            {
                var booking = await _bookingRepository.GetByOrderCode(webhookData.Data.OrderCode);
                if (booking != null)
                {
                    booking.PaymentStatus = PaymentStatus.Paid;
                    await _bookingRepository.UpdateAsync(booking);
                }
            }
        }

        public async Task<BookingSessionDto> HandlePayOSReturn(long orderCode)
        {
            var booking = await _bookingRepository.GetByOrderCode(orderCode);
            if (booking != null)
            {
                booking.PaymentStatus = PaymentStatus.Success;
                await _bookingRepository.UpdateAsync(booking);
                return new BookingSessionDto
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
                    Status = booking.Status,
                    PaymentStatus = booking.PaymentStatus,
                    OrderCode = (int)booking.OrderCode
                };
            }
            return null;
        }

        public async Task<BookingSessionDto> HandlePayOSCancel(long orderCode)
        {
            var booking = await _bookingRepository.GetByOrderCode(orderCode);
            if (booking != null)
            {
                booking.PaymentStatus = PaymentStatus.Cancelled;
                await _bookingRepository.UpdateAsync(booking);
                return new BookingSessionDto
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
                    Status = booking.Status,
                    PaymentStatus = booking.PaymentStatus,
                    OrderCode = (int)booking.OrderCode
                };
            }
            return null;
        }
    }
}