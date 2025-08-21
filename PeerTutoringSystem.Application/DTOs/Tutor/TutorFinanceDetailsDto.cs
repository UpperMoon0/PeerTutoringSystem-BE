using PeerTutoringSystem.Application.DTOs.Booking;
using System;
using System.Collections.Generic;

namespace PeerTutoringSystem.Application.DTOs.Tutor
{
    public class TutorFinanceDetailsDto
    {
        public IEnumerable<BookingSessionDto> Bookings { get; set; }
        public double TotalProfit { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; }
        public List<ChartDataPointDto> EarningsOverTime { get; set; }
        public double CurrentMonthEarnings { get; set; }
        public double LastMonthEarnings { get; set; }
        public double LifetimeEarnings { get; set; }
    }

    public class TransactionDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
    }

    public class ChartDataPointDto
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }
}