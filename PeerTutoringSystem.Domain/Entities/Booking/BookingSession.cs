using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using System;

namespace PeerTutoringSystem.Domain.Entities.Booking
{
    public class BookingSession
    {
        public Guid BookingId { get; set; }
        public Guid StudentId { get; set; }
        public virtual User Student { get; set; }
        public Guid TutorId { get; set; }
        public virtual User Tutor { get; set; }
        public Guid AvailabilityId { get; set; }
        public DateTime SessionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string? Topic { get; set; }
        public string? Description { get; set; }
        public BookingStatus Status { get; set; }
        public double basePrice { get; set; }
        public double serviceFee { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long OrderCode { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed,
        Rejected
    }
}