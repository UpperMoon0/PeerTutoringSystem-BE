using System;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    public class CreatePaymentDto
    {
        public Guid BookingId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }
}