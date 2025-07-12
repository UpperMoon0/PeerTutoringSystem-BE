namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class TutorVerificationDto
    {
        public Guid VerificationID { get; set; }
        public Guid UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string CitizenID { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string AdminNotes { get; set; } = string.Empty;
        public List<DocumentResponseDto> Documents { get; set; } = new();
    }
}