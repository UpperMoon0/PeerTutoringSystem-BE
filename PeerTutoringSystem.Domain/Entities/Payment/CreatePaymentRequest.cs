public class CreatePaymentRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string ReturnUrl { get; set; }
}