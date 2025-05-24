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

    public class BookingSessionDto
    {
        public Guid BookingId { get; set; }
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }
        public DateTime SessionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TutorName { get; set; }
        public string StudentName { get; set; }
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
    }

    public class InstantBookingDto
    {
        public Guid TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }

    public class BookingFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Status { get; set; }
        public Guid? SkillId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}