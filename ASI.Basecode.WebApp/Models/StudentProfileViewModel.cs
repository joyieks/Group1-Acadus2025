using System;

namespace ASI.Basecode.WebApp.Models
{
    public class StudentProfileViewModel
    {
        // Basic Profile Information
        public string? ProfileImageUrl { get; set; }
        public string? FullName { get; set; }
        public string? StudentId { get; set; }
        public string? Program { get; set; }
        public string? YearLevel { get; set; }

        // Personal Info
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        // Security Info
        public DateTime PasswordLastUpdated { get; set; }

        // For empty data handling
        // Automatically checks if profile data exists
        public bool HasData =>
            !string.IsNullOrWhiteSpace(FullName) ||
            !string.IsNullOrWhiteSpace(StudentId) ||
            !string.IsNullOrWhiteSpace(Program) ||
            !string.IsNullOrWhiteSpace(EmailAddress) ||
            !string.IsNullOrWhiteSpace(PhoneNumber);
    }
}
