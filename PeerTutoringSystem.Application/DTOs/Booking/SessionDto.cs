namespace PeerTutoringSystem.Application.DTOs
{
    public class SessionDto
    {
        public Guid SessionId { get; set; }
        public Guid BookingId { get; set; }
        public string VideoCallLink { get; set; }
        public string SessionNotes { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateSessionDto
    {
        public Guid BookingId { get; set; }
        public string VideoCallLink { get; set; }
        public string SessionNotes { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

    public class UpdateSessionDto
    {
        public string VideoCallLink { get; set; }
        public string SessionNotes { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}