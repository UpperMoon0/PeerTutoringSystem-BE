using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        public Guid BookingId { get; set; }
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
    }
}
