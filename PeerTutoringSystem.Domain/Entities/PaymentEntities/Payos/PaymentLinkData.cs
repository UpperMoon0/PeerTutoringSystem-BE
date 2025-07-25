namespace PeerTutoringSystem.Domain.Entities.PaymentEntities.Payos
{
    public class PaymentLinkData
    {
        public string Bin { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public string PaymentLinkId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public int OrderCode { get; set; }
        public long ExpiredAt { get; set; }
        public string Status { get; set; }
        public string CheckoutUrl { get; set; }
        public string QrCode { get; set; }
    }
}