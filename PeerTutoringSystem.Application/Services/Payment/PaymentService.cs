using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Authentication;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly ILogger<PaymentService> _logger;
        private readonly IUserBioRepository _userBioRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IBookingSessionRepository bookingRepository,
            ILogger<PaymentService> logger,
            IUserBioRepository userBioRepository,
            ISessionRepository sessionRepository,
            IUserRepository userRepository)
        {
            _paymentRepository = paymentRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _bookingRepository = bookingRepository;
            _logger = logger;
            _userBioRepository = userBioRepository;
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentHistory(string userId)
        {
            var paymentHistory = await _paymentRepository.GetPaymentHistory(userId);
            return paymentHistory.Select(p => new PaymentDto
            {
                Id = p.Id,
                TransactionId = p.TransactionId,
                BookingId = p.BookingId,
                Amount = p.Amount,
                TransactionDate = p.CreatedAt,
                Status = p.Status.ToString(),
                TutorName = p.Booking.Tutor.FullName,
                StudentName = p.Booking.Student.FullName
            });
        }

        public async Task<PayOSCreatePaymentLinkResponseDto> CreatePaymentLink(PayOSCreatePaymentLinkRequestDto request)
        {
            var successUrl = _configuration["PayOS:SuccessUrl"];
            var cancelUrl = _configuration["PayOS:CancelUrl"];
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
            _logger.LogInformation("==============================================");
            _logger.LogInformation("[PAYOS WEBHOOK] Handling webhook...");
            _logger.LogInformation("==============================================");
            var checksumKey = Environment.GetEnvironmentVariable("PayOS_Checksum_Key");
            if (string.IsNullOrEmpty(checksumKey))
            {
                _logger.LogError("[PAYOS WEBHOOK] Checksum Key is not configured.");
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
                _logger.LogWarning("[PAYOS WEBHOOK] Invalid signature.");
                throw new Exception("Invalid signature");
            }

            _logger.LogInformation("[PAYOS WEBHOOK] Signature verified.");

            if (webhookData.Code == "00")
            {
                _logger.LogInformation("[PAYOS WEBHOOK] Payment successful, updating booking status.");
                var booking = await _bookingRepository.GetByOrderCode(webhookData.Data.OrderCode);
                if (booking != null)
                {
                    booking.PaymentStatus = PaymentStatus.Paid;
                    await _bookingRepository.UpdateAsync(booking);
                    _logger.LogInformation("[PAYOS WEBHOOK] Booking status updated to Paid for OrderCode {OrderCode}", webhookData.Data.OrderCode);

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
                                tutor.AccountBalance += amountToPay;
                                await _userRepository.UpdateAsync(tutor);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("[PAYOS WEBHOOK] Booking not found for OrderCode {OrderCode}", webhookData.Data.OrderCode);
                }
            }
            else
            {
                _logger.LogInformation("[PAYOS WEBHOOK] Payment not successful. Code: {Code}", webhookData.Code);
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

        public async Task<FinanceDetailsDto> GetFinanceDetails()
        {
            var allBookings = await _bookingRepository.GetAllAsync();
            var paidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid);

            var totalPayments = paidBookings.Count();
            var totalIncome = paidBookings.Sum(b => b.basePrice + b.serviceFee);
            var totalProfit = paidBookings.Sum(b => b.serviceFee);

            return new FinanceDetailsDto
            {
                TotalPayments = totalPayments,
                TotalIncome = totalIncome,
                TotalProfit = totalProfit
            };
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPayments()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                TransactionId = p.TransactionId,
                BookingId = p.BookingId,
                Amount = p.Amount,
                TransactionDate = p.CreatedAt,
                Status = p.Status.ToString(),
                TutorName = p.Booking.Tutor.FullName,
                StudentName = p.Booking.Student.FullName
            });
        }
    }
}