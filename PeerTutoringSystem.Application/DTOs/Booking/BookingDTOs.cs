namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class TutorAvailabilityDto
    {
        public Guid AvailabilityId { get; set; }
        public Guid TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsDailyRecurring { get; set; } 
        public string RecurringDay { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public bool IsBooked { get; set; }
    }

    public class CreateTutorAvailabilityDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsDailyRecurring { get; set; }
        public string RecurringDay { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
    }

    public class CreateBookingDto
    {
        public Guid TutorId { get; set; }
        public Guid AvailabilityId { get; set; }
        public Guid? SkillId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        public string Status { get; set; }
        public string? PaymentStatus { get; set; }
    }

    // DTO cho yêu cầu đặt lịch tức thời
    public class InstantBookingDto
    {
        public Guid TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}