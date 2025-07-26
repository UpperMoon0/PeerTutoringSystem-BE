using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Infrastructure.Data;

namespace PeerTutoringSystem.Infrastructure.Repositories.Payment
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _dbContext;

        public PaymentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<PaymentEntity> CreatePaymentAsync(PaymentEntity payment)
        {
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
            return payment;
        }

        public async Task<PaymentEntity> GetPaymentByIdAsync(Guid id)
        {
            return await _dbContext.Payments
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentEntity> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _dbContext.Payments
                .Where(p => p.TransactionId == transactionId)
                .FirstOrDefaultAsync();
        }
        public async Task<PaymentEntity> GetPaymentByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.Payments
                .Where(p => p.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PaymentEntity>> GetPaymentsByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.Payments
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<PaymentEntity> UpdatePaymentAsync(PaymentEntity payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _dbContext.Payments.Update(payment);
            await _dbContext.SaveChangesAsync();
            return payment;
        }
        public async Task<IEnumerable<PaymentEntity>> GetAllAsync()
        {
            return await _dbContext.Payments.ToListAsync();
        }
    }
}