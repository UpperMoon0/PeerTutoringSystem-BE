using PeerTutoringSystem.Domain.Entities.Authentication;

namespace PeerTutoringSystem.Domain.Entities.Profile_Bio
{
    public class UserBio 
    {
        public int BioID { get; set; } 
        public Guid UserID { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public string Availability { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}