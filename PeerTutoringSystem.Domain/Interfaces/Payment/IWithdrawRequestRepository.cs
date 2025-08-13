using PeerTutoringSystem.Domain.Common;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Domain.Interfaces.Payment
{
    public interface IWithdrawRequestRepository : IRepository<WithdrawRequest>
    {
        Task UpdateAsync(WithdrawRequest withdrawRequest);
    }
}