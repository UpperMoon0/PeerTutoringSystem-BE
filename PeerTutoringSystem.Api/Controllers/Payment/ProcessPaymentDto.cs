using System;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    public class ProcessPaymentDto
    {
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}