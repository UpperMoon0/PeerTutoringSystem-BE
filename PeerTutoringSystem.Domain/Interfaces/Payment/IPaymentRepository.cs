using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Domain.Interfaces.Payment
{
    public interface IPaymentRepository
    {
        Task<PaymentEntity> CreatePaymentAsync(PaymentEntity payment);
        Task<PaymentEntity> GetPaymentByIdAsync(Guid id);
        Task<PaymentEntity> GetPaymentByTransactionIdAsync(string transactionId);
        Task<PaymentEntity> GetPaymentByBookingIdAsync(Guid bookingId);
        Task<List<PaymentEntity>> GetPaymentsByBookingIdAsync(Guid bookingId);
        Task<PaymentEntity> UpdatePaymentAsync(PaymentEntity payment);
        Task<IEnumerable<PaymentEntity>> GetAllAsync();
    }
}