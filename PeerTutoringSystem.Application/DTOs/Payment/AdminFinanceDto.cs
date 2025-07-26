using System;
using System.Collections.Generic;

namespace PeerTutoringSystem.Application.DTOs.Payment
{
    public class AdminFinanceDto
    {
        public double TotalRevenue { get; set; }
        public double AverageTransactionValue { get; set; }
        public int TotalTransactions { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
        public List<RecentTransactionDto> RecentTransactions { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public double Revenue { get; set; }
    }

    public class RecentTransactionDto
    {
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public Guid BookingId { get; set; }
    }
}