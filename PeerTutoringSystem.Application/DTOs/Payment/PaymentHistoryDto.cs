using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class PaymentHistoryDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string TutorName { get; set; }
        public string StudentName { get; set; }
    }
}