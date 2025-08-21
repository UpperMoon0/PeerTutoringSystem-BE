using System;
using PeerTutoringSystem.Domain.Entities.Booking;

namespace PeerTutoringSystem.Domain.Entities.PaymentEntities
{
    public class PaymentEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookingId { get; set; }
        public string? TransactionId { get; set; }  // External transaction ID from PayOS
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string? Description { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaymentUrl { get; set; }  // URL for redirect to payment gateway
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property with virtual keyword for lazy loading
        public virtual BookingSession? Booking { get; set; }
    }
}