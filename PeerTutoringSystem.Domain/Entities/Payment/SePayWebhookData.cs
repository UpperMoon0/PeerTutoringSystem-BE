public class SePayWebhookData
{
    public string TransactionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } 
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime Timestamp { get; set; }
    // Add other fields based on SePay webhook payload
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed
}