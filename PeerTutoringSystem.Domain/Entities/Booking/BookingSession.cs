using System;

namespace PeerTutoringSystem.Domain.Entities.Booking
{
    public class BookingSession
    {
        public Guid BookingId { get; set; }
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }
        public Guid AvailabilityId { get; set; }
        public DateTime SessionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string? Topic { get; set; }
        public string? Description { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed,
        Rejected
    }
    public enum PaymentStatus
    {
        Unpaid,
        Processing,
        Paid
    }
}