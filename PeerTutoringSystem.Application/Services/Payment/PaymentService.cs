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

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IPaymentRepository _paymentRepository;
        // If you're using SignalR for real-time notifications
        // private readonly IHubContext<PaymentHub> _paymentHubContext;

        public PaymentService(
            HttpClient httpClient, 
            IConfiguration config, 
            IPaymentRepository paymentRepository)
            // IHubContext<PaymentHub> paymentHubContext)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            // _paymentHubContext = paymentHubContext ?? throw new ArgumentNullException(nameof(paymentHubContext));
            
            _httpClient.BaseAddress = new Uri(_config["SePay:BaseUrl"]);
            if (!string.IsNullOrEmpty(_config["SePay:ApiKey"]))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _config["SePay:ApiKey"]);
            }
        }

        public async Task<PaymentResponse> CreatePayment(CreatePaymentRequest request)
        {
            try
            {
                // Create payment request for SePay
                var sePayRequest = new
                {
                    amount = request.Amount,
                    description = request.Description,
                    returnUrl = request.ReturnUrl,
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
                    TransactionId = sePayResponse.transactionId.ToString(),
                    Amount = request.Amount,
                    Description = request.Description,
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
                // Get payment by transaction ID
                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(webhookData.Id.ToString());

                if (payment != null)
                {
                    // Determine payment status based on webhook data
                    PaymentStatus status;
                    
                    // Logic to determine payment status from webhookData
                    // For example, if "transferType" is "in", payment is successful
                    if (webhookData.TransferType?.ToLower() == "in")
                    {
                        status = PaymentStatus.Success;
                    }
                    else
                    {
                        status = PaymentStatus.Failed;
                    }
                    
                    // Update payment details
                    payment.Status = status;
                    payment.UpdatedAt = DateTime.UtcNow;
                    
                    // Save to database
                    await _paymentRepository.UpdatePaymentAsync(payment);

                    // Notify frontend via SignalR
                    await NotifyPaymentStatusChange(payment.Id, status);
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