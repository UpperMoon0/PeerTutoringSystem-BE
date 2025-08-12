namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class CreateBookingDto
    {
        public Guid TutorId { get; set; }
        public Guid AvailabilityId { get; set; }
        public Guid? SkillId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}