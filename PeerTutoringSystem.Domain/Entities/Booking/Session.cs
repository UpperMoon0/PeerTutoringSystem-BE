using System;

namespace PeerTutoringSystem.Domain.Entities.Booking
{
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid BookingId { get; set; }
        public string VideoCallLink { get; set; }
        public string SessionNotes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}