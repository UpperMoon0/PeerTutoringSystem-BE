using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using System;

namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class BookingSessionDto
    {
        public Guid BookingId { get; set; }
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }
        public DateTime SessionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string? Topic { get; set; }
        public string? Description { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public long OrderCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StudentName { get; set; }
        public string TutorName { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? ServiceFee { get; set; }
    }
}