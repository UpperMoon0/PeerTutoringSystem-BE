namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PaymentId { get; set; }
        public string QrCode { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ServiceFee { get; set; }
    }
}