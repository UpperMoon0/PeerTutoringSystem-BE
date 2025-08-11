using System;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class TransactionHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}