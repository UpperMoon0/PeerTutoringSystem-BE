using System;

namespace PeerTutoringSystem.Application.DTOs.Reviews
{
    public class ReviewDto
    {
        public int ReviewID { get; set; }
        public Guid BookingID { get; set; }
        public Guid StudentID { get; set; }
        public Guid TutorID { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public string StudentName { get; set; }
        public string StudentAvatarUrl { get; set; }
    }
}
