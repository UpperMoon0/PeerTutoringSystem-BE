using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;

namespace PeerTutoringSystem.Application.Services.Payment
{
   public class PaymentService : IPaymentService
   {
       private readonly HttpClient _httpClient;
       private readonly IConfiguration _config;
       private readonly IPaymentRepository _paymentRepository;
       private readonly IBookingSessionRepository _bookingRepository;
       private readonly IUserBioRepository _userBioRepository;
       private readonly ISessionRepository _sessionRepository;
       private readonly bool _simulatePayment;

       public PaymentService(
           HttpClient httpClient,
           IConfiguration config,
           IPaymentRepository paymentRepository,
           IBookingSessionRepository bookingRepository,
           IUserBioRepository userBioRepository,
           ISessionRepository sessionRepository)
       // IHubContext<PaymentHub> paymentHubContext)
       {
           _config = config ?? throw new ArgumentNullException(nameof(config));
           _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
           _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
           _userBioRepository = userBioRepository ?? throw new ArgumentNullException(nameof(userBioRepository));
           _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
           // _paymentHubContext = paymentHubContext ?? throw new ArgumentNullException(nameof(paymentHubContext));

           _simulatePayment = _config.GetValue<bool>("SePay:SimulatePayment");

           if (!_simulatePayment)
           {
               _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
               _httpClient.BaseAddress = new Uri(_config["SePay:BaseUrl"]);
               if (!string.IsNullOrEmpty(_config["SePay:ApiKey"]))
               {
                   _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _config["SePay:ApiKey"]);
               }
           }
       }

       public async Task<PaymentResponse> CreatePayment(Guid bookingId, string returnUrl)
       {
           try
           {
               var booking = await _bookingRepository.GetByIdAsync(bookingId);
               if (booking == null)
               {
                   return new PaymentResponse { Success = false, Message = "Booking not found." };
               }

               var tutorBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
               if (tutorBio == null)
               {
                   return new PaymentResponse { Success = false, Message = "Tutor profile not found." };
               }

               var session = await _sessionRepository.GetByBookingIdAsync(bookingId);
               if (session == null)
               {
                   return new PaymentResponse { Success = false, Message = "Session not found." };
               }
               var durationHours = (session.EndTime - session.StartTime).TotalHours;
               var amount = (decimal)durationHours * tutorBio.HourlyRate;
               var description = $"Payment for booking {booking.BookingId}";

               if (_simulatePayment)
               {
                   // Simulate a successful payment
                   var simulatedPayment = new PaymentEntity
                   {
                       BookingId = bookingId,
                       TransactionId = $"SIM_{Guid.NewGuid()}",
                       Amount = amount,
                       Description = description,
                       PaymentUrl = "", // No external payment URL in simulation
                       Status = PaymentStatus.Success,
                       CreatedAt = DateTime.UtcNow,
                       UpdatedAt = DateTime.UtcNow
                   };

                   await _paymentRepository.CreatePaymentAsync(simulatedPayment);

                   // Update booking status
                   booking.PaymentStatus = Domain.Entities.Booking.PaymentStatus.Paid;
                   await _bookingRepository.UpdateAsync(booking);

                   return new PaymentResponse
                   {
                       Success = true,
                       PaymentId = simulatedPayment.Id.ToString(),
                       PaymentUrl = returnUrl, // Redirect back to the provided return URL
                       TransactionId = simulatedPayment.TransactionId,
                       Amount = simulatedPayment.Amount,
                       Message = "Payment simulated successfully"
                   };
               }

               // Create payment request for SePay
               var sePayRequest = new
               {
                   amount = amount,
                   description = description,
                   returnUrl = returnUrl,
                   // Add any other required parameters
                   merchantId = _config["SePay:MerchantId"],
                   currency = "VND"
               };

               // Serialize the request
               var content = new StringContent(
                   JsonSerializer.Serialize(sePayRequest),
                   Encoding.UTF8,
                   "application/json");

               // Send request to SePay API
               var response = await _httpClient.PostAsync(_config["SePay:PaymentEndpoint"], content);
               response.EnsureSuccessStatusCode();

               // Deserialize the response
               var sePayResponse = await response.Content.ReadFromJsonAsync<dynamic>();

               // Create local payment record
               var payment = new PaymentEntity
               {
                   BookingId = bookingId,
                   TransactionId = sePayResponse.transactionId.ToString(),
                   Amount = amount,
                   Description = description,
                   PaymentUrl = sePayResponse.paymentUrl.ToString(),
                   Status = PaymentStatus.Pending,
                   CreatedAt = DateTime.UtcNow
               };

               // Save payment to database
               await _paymentRepository.CreatePaymentAsync(payment);

               // Return response to client
               return new PaymentResponse
               {
                   Success = true,
                   PaymentId = payment.Id.ToString(),
                   PaymentUrl = payment.PaymentUrl,
                   TransactionId = payment.TransactionId,
                   Amount = payment.Amount,
                   Message = "Payment created successfully"
               };
           }
           catch (Exception ex)
           {
               // Handle exceptions
               return new PaymentResponse
               {
                   Success = false,
                   Message = $"Failed to create payment: {ex.Message}"
               };
           }
       }

        public async Task<PaymentStatus> GetPaymentStatus(string paymentId)
        {
            try
            {
                if (Guid.TryParse(paymentId, out var id))
                {
                    var payment = await _paymentRepository.GetPaymentByIdAsync(id);
                    return payment?.Status ?? PaymentStatus.Failed;
                }

                return PaymentStatus.Failed;
            }
            catch
            {
                return PaymentStatus.Failed;
            }
        }

        public async Task ProcessPaymentWebhook(SePayWebhookData webhookData)
        {
            try
            {
                Console.WriteLine($"Received SePay Webhook: {System.Text.Json.JsonSerializer.Serialize(webhookData)}");

                // Get payment by transaction ID
                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(webhookData.Id.ToString());

                if (payment != null)
                {
                    Console.WriteLine($"Found payment record for Transaction ID {webhookData.Id}. Current Status: {payment.Status}");

                    // Determine payment status based on webhook data
                    PaymentStatus newStatus;

                    // Logic to determine payment status from webhookData
                    // For example, if "transferType" is "in", payment is successful
                    if (webhookData.TransferType?.ToLower() == "in")
                    {
                        newStatus = PaymentStatus.Success;
                    }
                    else
                    {
                        newStatus = PaymentStatus.Failed;
                    }
                    Console.WriteLine($"Determined new status: {newStatus} for Transaction ID {webhookData.Id}");

                    if (payment.Status != newStatus)
                    {
                        // Update payment details
                        payment.Status = newStatus;
                        payment.UpdatedAt = DateTime.UtcNow;

                        // Save to database
                        await _paymentRepository.UpdatePaymentAsync(payment);
                        Console.WriteLine($"Payment status updated to {newStatus} for Transaction ID {webhookData.Id}");

                        // Notify frontend via SignalR
                        await NotifyPaymentStatusChange(payment.Id, newStatus);
                        if (newStatus == PaymentStatus.Success)
                        {
                            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
                            if (booking != null)
                            {
                                booking.PaymentStatus = Domain.Entities.Booking.PaymentStatus.Paid;
                                await _bookingRepository.UpdateAsync(booking);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Payment status for Transaction ID {webhookData.Id} is already {newStatus}. No update performed.");
                    }
                }
                else
                {
                    Console.WriteLine($"No payment record found for Transaction ID {webhookData.Id}. Webhook data: {System.Text.Json.JsonSerializer.Serialize(webhookData)}");
                    // Consider how to handle webhooks for unknown transaction IDs based on business rules.
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error processing payment webhook: {ex.Message}");
                throw;
            }
        }

        private async Task NotifyPaymentStatusChange(Guid paymentId, PaymentStatus status)
        {
            // If using SignalR, notify clients about payment status change
            // await _paymentHubContext.Clients.All.SendAsync("PaymentStatusChanged", paymentId, status);

            // For now, just return a completed task if SignalR is not implemented
            await Task.CompletedTask;
        }
    }
}