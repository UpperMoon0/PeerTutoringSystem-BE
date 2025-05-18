// PeerTutoringSystem.Domain/Entities/Booking/TutorAvailability.cs
using System;

namespace PeerTutoringSystem.Domain.Entities.Booking
{
    public class TutorAvailability
    {
        public Guid AvailabilityId { get; set; }
        public Guid TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public DayOfWeek? RecurringDay { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public bool IsBooked { get; set; }
    }
}
