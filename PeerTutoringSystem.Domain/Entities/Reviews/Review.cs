using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Entities.Booking;
using System;

namespace PeerTutoringSystem.Domain.Entities.Reviews
{
    public class Review
    {
        public int ReviewID { get; set; }
        public Guid BookingID { get; set; }
        public Guid StudentID { get; set; }
        public Guid TutorID { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }

        // Navigation properties
        public BookingSession Booking { get; set; }
        public User Student { get; set; }
        public User Tutor { get; set; }
    }
}