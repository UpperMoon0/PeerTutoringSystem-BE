using System;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class PaymentConfirmationDto
    {
        public Guid BookingId { get; set; }
        public string Status { get; set; }
    }
}