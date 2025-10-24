using System;

namespace ASI.Basecode.WebApp.Models
{
    public class StudentProfileViewModel
    {
        // ===== Basic Profile Information =====
        public string? ProfileImageUrl { get; set; }
        public string? FullName { get; set; }
        public string? StudentId { get; set; }

        // ===== Personal Info =====
        public string? Status { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Suffix { get; set; }

        public string? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public string? Course { get; set; }
        public string? YearLevel { get; set; }
        public string? Department { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }

        // ===== Address Information =====
        public string? HouseNumber { get; set; }
        public string? Street { get; set; }
        public string? Subdivision { get; set; }
        public string? Barangay { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }

        // ===== Emergency Contact Information =====
        public string? EmergencyFirstName { get; set; }
        public string? EmergencyMiddleName { get; set; }
        public string? EmergencyLastName { get; set; }
        public string? EmergencySuffix { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? EmergencyRelationship { get; set; }

        // ===== Security Info =====
        public DateTime PasswordLastUpdated { get; set; }


        // ===== Data Check =====
        public bool HasData =>
            !string.IsNullOrWhiteSpace(FullName) ||
            !string.IsNullOrWhiteSpace(StudentId) ||
            !string.IsNullOrWhiteSpace(EmailAddress) ||
            !string.IsNullOrWhiteSpace(PhoneNumber);
    }
}
