using Microsoft.Extensions.Configuration;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    // Store payment status in database or cache
    public async Task ProcessPaymentWebhook(SePayWebhookData webhookData)
    {
        // Update payment status in your database
        var payment = await GetPaymentByTransactionId(webhookData.TransactionId);

        if (payment != null)
        {
            payment.Status = webhookData.Status;
            payment.UpdatedAt = DateTime.UtcNow;
            // Save to database

            // Optionally notify frontend via SignalR
            await NotifyPaymentStatusChange(payment.Id, webhookData.Status);
        }
    }
}