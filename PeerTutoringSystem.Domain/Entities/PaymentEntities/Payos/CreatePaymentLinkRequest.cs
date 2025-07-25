using System.Collections.Generic;

namespace PeerTutoringSystem.Domain.Entities.PaymentEntities.Payos
{
    public class CreatePaymentLinkRequest
    {
        public int OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerAddress { get; set; }
        public List<Item> Items { get; set; }
        public string CancelUrl { get; set; }
        public string ReturnUrl { get; set; }
        public long? ExpiredAt { get; set; }
        public string Signature { get; set; }
    }
}