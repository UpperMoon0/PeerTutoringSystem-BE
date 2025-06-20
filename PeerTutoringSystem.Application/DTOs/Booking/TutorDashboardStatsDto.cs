namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class TutorDashboardStatsDto
    {
        public int TotalBookings { get; set; }
        public int AvailableSlots { get; set; }
        public int CompletedSessions { get; set; }
        public decimal TotalEarnings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
    }
}