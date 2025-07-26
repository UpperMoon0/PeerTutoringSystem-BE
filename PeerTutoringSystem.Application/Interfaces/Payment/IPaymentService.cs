using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
 
 public interface IPaymentService
 {
     Task<PaymentResponseDto> CreatePayment(CreatePaymentRequestDto request);
     Task<PaymentStatus> GetPaymentStatus(string paymentId);
     Task<bool> ConfirmPayment(Guid bookingId);
    Task<AdminFinanceDto> GetAdminFinanceDetails();
    Task ProcessPaymentWebhook(SePayWebhookData webhookData);
 }
