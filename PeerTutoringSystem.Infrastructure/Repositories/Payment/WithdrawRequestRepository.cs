using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Infrastructure.Data;
using PeerTutoringSystem.Infrastructure.Repositories;

namespace PeerTutoringSystem.Infrastructure.Repositories.Payment
{
    public class WithdrawRequestRepository : Repository<WithdrawRequest>, IWithdrawRequestRepository
    {
        public WithdrawRequestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task UpdateAsync(WithdrawRequest withdrawRequest)
        {
            _context.Update(withdrawRequest);
            await _context.SaveChangesAsync();
        }
    }
}