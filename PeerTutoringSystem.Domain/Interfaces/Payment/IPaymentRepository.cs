using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Payment
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<PaymentEntity>> GetPaymentHistory(string userId);
        Task AddAsync(PaymentEntity payment);
    }
}