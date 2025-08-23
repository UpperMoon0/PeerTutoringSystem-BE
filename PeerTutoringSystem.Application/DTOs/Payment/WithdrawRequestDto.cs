using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class WithdrawRequestDto
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public WithdrawRequestStatus Status { get; set; }
    }

    public class CreateWithdrawRequestDto
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
    }

    public class UpdateWithdrawRequestDto
    {
        public WithdrawRequestStatus Status { get; set; }
    }
}