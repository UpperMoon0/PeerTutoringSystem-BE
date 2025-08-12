using System.Collections.Generic;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class PayOSCreatePaymentLinkRequestDto
    {
        public Guid BookingId { get; set; }
    }

    public class PayOSItemDto
    {
        public string name { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }

    public class PayOSCreatePaymentLinkResponseDto
    {
        public string code { get; set; }
        public string desc { get; set; }
        public PayOSCreatePaymentLinkDataDto data { get; set; }
        public string signature { get; set; }
    }

    public class PayOSCreatePaymentLinkDataDto
    {
        public string bin { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public long orderCode { get; set; }
        public string paymentLinkId { get; set; }
        public string status { get; set; }
        public string checkoutUrl { get; set; }
        public string qrCode { get; set; }
        public decimal basePrice { get; set; }
        public decimal serviceFee { get; set; }
    }
}