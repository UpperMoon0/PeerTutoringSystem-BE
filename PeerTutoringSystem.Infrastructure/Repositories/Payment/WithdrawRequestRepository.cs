using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Infrastructure.Data;

namespace PeerTutoringSystem.Infrastructure.Repositories.Payment
{
    public class WithdrawRequestRepository : Repository<WithdrawRequest>, IWithdrawRequestRepository
    {
        public WithdrawRequestRepository(AppDbContext context) : base(context)
        {
        }
    }
}