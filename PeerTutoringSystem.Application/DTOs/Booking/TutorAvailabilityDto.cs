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
}