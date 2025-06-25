using System;

namespace PeerTutoringSystem.Application.DTOs.Reviews
{
    public class TutorRatingDto
    {
        public Guid TutorId { get; set; }
        public string TutorName { get; set; }
        public string Email { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}