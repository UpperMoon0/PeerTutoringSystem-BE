using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePayment(Guid bookingId, string returnUrl);
    Task ProcessPaymentWebhook(SePayWebhookData webhookData);
    Task<PaymentStatus> GetPaymentStatus(string paymentId);
    Task<string> CreatePaymentLink(int orderCode, int amount, string description, string returnUrl, string cancelUrl);
}
