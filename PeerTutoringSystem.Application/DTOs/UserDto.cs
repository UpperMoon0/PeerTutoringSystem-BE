using System;
using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Application.DTOs
{
    public class UserDto
    {
        public Guid UserID { get; set; }
        public string AnonymousName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Hometown { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Anonymous name is required.")]
        public string AnonymousName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("Male|Female|Other", ErrorMessage = "Gender must be 'Male', 'Female', or 'Other'.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hometown is required.")]
        public string Hometown { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty; // Optional
    }
}