namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class UpdateTutorAvailabilityDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsDailyRecurring { get; set; }
        public string? RecurringDay { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
    }
}