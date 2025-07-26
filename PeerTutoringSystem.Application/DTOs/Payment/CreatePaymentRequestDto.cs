namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class CreatePaymentRequestDto
    {
        public Guid BookingId { get; set; }
        public string ReturnUrl { get; set; }
    }
}