using System;

namespace PeerTutoringSystem.Domain.Entities
{
    public class UserToken
    {
        public Guid TokenID { get; set; }
        public Guid UserID { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public User? User { get; set; }
    }
}