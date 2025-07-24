namespace PeerTutoringSystem.Domain.Entities.PaymentEntities.Payos
{
    public class CreatePaymentLinkResponse
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PaymentLinkData Data { get; set; }
        public string Signature { get; set; }
    }
}