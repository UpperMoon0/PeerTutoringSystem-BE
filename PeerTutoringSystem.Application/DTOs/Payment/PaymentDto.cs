namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? TutorName { get; set; }
        public string? StudentName { get; set; }
    }
}