namespace PeerTutoringSystem.Application.DTOs.Authentication
{
    public class DocumentDto
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}