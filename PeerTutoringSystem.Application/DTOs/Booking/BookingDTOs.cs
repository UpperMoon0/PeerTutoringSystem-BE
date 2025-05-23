
namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class TutorAvailabilityDto
    {
        public Guid AvailabilityId { get; set; }
        public Guid TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrenceRule { get; set; } // e.g., "Weekly:Monday,Wednesday"
        public DateTime? RecurrenceEndDate { get; set; }
        public bool IsBooked { get; set; }
        public bool AllowInstantBooking { get; set; }
    }

    public class CreateTutorAvailabilityDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrenceRule { get; set; } // e.g., "Weekly:Monday,Wednesday"
        public DateTime? RecurrenceEndDate { get; set; }
        public bool AllowInstantBooking { get; set; }
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
        public string Status { get; set; } // "Confirmed", "Cancelled", "Completed"
    }

    public class BookingFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Status { get; set; } // e.g., "Pending,Confirmed,Cancelled,Completed"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? TutorId { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}