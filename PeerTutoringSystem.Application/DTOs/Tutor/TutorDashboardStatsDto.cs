namespace PeerTutoringSystem.Application.DTOs.Tutor
{
    public class TutorDashboardStatsDto
    {
        public int TotalBookings { get; set; }
        public int AvailableSlots { get; set; }
        public int CompletedSessions { get; set; }
        public double TotalEarnings { get; set; }
    }
}