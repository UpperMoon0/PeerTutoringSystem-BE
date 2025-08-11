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

        public async Task<IEnumerable<PaymentEntity>> GetPaymentHistory()
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