namespace PeerTutoringSystem.Application.DTOs.Booking
{
    public class TutorSessionStatsDto
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int CanceledSessions { get; set; }
        public double TotalHours { get; set; }
    }
}