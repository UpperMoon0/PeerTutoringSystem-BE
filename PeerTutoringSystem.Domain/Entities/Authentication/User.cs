using System;

namespace PeerTutoringSystem.Domain.Entities.Authentication
{
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum UserStatus
    {
        Active,
        Banned
    }

    public class User
    {
        public Guid UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string FirebaseUid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string Hometown { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastActive { get; set; }
        public bool IsOnline { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public int RoleID { get; set; }
        public Role Role { get; set; }
    }
}