using Microsoft.Extensions.Configuration;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePayment(CreatePaymentRequest request);
    Task ProcessPaymentWebhook(SePayWebhookData webhookData);
    Task<PaymentStatus> GetPaymentStatus(string paymentId);
}
