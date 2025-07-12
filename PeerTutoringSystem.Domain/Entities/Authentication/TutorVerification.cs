using System;

namespace PeerTutoringSystem.Domain.Entities.Authentication
{
    public class TutorVerification
    {
        public Guid VerificationID { get; set; }
        public Guid UserID { get; set; }
        public string? CitizenID { get; set; }
        public string? StudentID { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public string? VerificationStatus { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? AdminNotes { get; set; }
        public string? AccessLevel { get; set; }
        
        // Fix: Ensure virtual keyword and properly handle null reference
        public virtual User? User { get; set; }
    }
}