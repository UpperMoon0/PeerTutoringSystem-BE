namespace PeerTutoringSystem.Application.DTOs.Booking
{
    // DTO cho yêu cầu đặt lịch tức thời
    public class InstantBookingDto
    {
        public Guid TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? SkillId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}