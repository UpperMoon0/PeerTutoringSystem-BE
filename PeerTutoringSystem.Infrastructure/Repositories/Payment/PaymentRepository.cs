using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories.Payment
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentEntity>> GetPaymentHistory(string userId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Tutor)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Student)
                .Where(p => p.Booking.Student.UserID.ToString() == userId || p.Booking.Tutor.UserID.ToString() == userId)
                .ToListAsync();
        }

        public async Task AddAsync(PaymentEntity payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaymentEntity>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Tutor)
                .Include(p => p.Booking)
                .ThenInclude(b => b.Student)
                .ToListAsync();
        }
    }
}