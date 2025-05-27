using System.Text.Json.Serialization;

namespace PeerTutoringSystem.Domain.Entities.PaymentEntities;

public class SePayWebhookData
{
    [JsonPropertyName("id")]
    public long Id { get; set; } // Changed from TransactionId (string) to long, matching SePay's 'id'

    [JsonPropertyName("gateway")]
    public string Gateway { get; set; }

    [JsonPropertyName("transactionDate")]
    public DateTime TransactionDate { get; set; } // Changed from Timestamp, maps to 'transactionDate'

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; } // Nullable if 'code' can be null

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("transferType")]
    public string TransferType { get; set; } // "in" or "out"

    [JsonPropertyName("transferAmount")]
    public decimal TransferAmount { get; set; } // Changed from Amount, maps to 'transferAmount'

    [JsonPropertyName("accumulated")]
    public decimal Accumulated { get; set; }

    [JsonPropertyName("subAccount")]
    public string? SubAccount { get; set; } // Nullable if 'subAccount' can be null

    [JsonPropertyName("referenceCode")]
    public string? ReferenceCode { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    // Existing PaymentStatus property. This is not directly from the SePay payload
    // but might be used internally by your application after processing the webhook.
    // Its value would likely be determined based on 'transferType' or other logic.
    public PaymentStatus PaymentStatus { get; set; }

}
