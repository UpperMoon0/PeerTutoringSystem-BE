using PeerTutoringSystem.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistory(string userId);
    }
}