using System.Text.Json.Serialization;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class PayOSWebhookData
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        [JsonPropertyName("data")]
        public PayOSWebhookInnerData Data { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }

    public class PayOSWebhookInnerData
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        [JsonPropertyName("transactionDateTime")]
        public string TransactionDateTime { get; set; }

        [JsonPropertyName("paymentLinkId")]
        public string PaymentLinkId { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }
    }
}