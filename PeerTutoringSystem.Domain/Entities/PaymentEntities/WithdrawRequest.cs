using PeerTutoringSystem.Domain.Common;
using PeerTutoringSystem.Domain.Entities.Authentication;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeerTutoringSystem.Domain.Entities.PaymentEntities
{
    public enum WithdrawRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Canceled
    }

    public class WithdrawRequest : BaseEntity
    {
        [ForeignKey(nameof(User))]
        public Guid TutorId { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public WithdrawRequestStatus Status { get; set; } = WithdrawRequestStatus.Pending;
        public virtual User Tutor { get; set; }
    }
}