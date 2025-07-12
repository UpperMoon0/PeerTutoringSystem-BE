using System;

namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class DocumentResponseDto
    {
        public Guid DocumentID { get; set; }
        public string DocumentPath { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public DateTime UploadDate { get; set; }
    }
}