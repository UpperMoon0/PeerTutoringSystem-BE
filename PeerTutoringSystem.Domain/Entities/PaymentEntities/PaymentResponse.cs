using System.Text.Json.Serialization;

namespace PeerTutoringSystem.Domain.Entities.PaymentEntities
{
    public class PaymentResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; }
        
        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; }
        
        [JsonPropertyName("paymentUrl")]
        public string PaymentUrl { get; set; }
        
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }
        
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}