using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistory()
        {
            var paymentHistory = await _paymentRepository.GetPaymentHistory();
            return paymentHistory.Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Date = p.CreatedAt,
                Status = p.Status.ToString(),
                TutorName = p.Booking.Tutor.FullName,
                StudentName = p.Booking.Student.FullName
            });
        }
    }
}