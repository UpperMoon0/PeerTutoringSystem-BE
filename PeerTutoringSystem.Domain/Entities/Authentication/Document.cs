using System;

namespace PeerTutoringSystem.Domain.Entities.Authentication
{
    public class Document
    {
        public Guid DocumentID { get; set; }
        public Guid VerificationID { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime UploadDate { get; set; }
        public int FileSize { get; set; }
        public string? AccessLevel { get; set; }
        public TutorVerification? TutorVerification { get; set; }
    }
}