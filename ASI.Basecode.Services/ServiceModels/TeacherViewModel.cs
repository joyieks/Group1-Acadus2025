using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class TeacherViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Suffix")]
        public string Suffix { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; }

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; }

        // Address Information
        [Display(Name = "House/Apt Number")]
        public string HouseNumber { get; set; }

        [Required]
        [Display(Name = "Street Name")]
        public string StreetName { get; set; }

        [Display(Name = "Subdivision/Building")]
        public string Subdivision { get; set; }

        [Required]
        [Display(Name = "Barangay")]
        public string Barangay { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Province")]
        public string Province { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        // Emergency Contact Information
        [Required]
        [Display(Name = "Emergency Contact First Name")]
        public string EmergencyFirstName { get; set; }

        [Required]
        [Display(Name = "Emergency Contact Last Name")]
        public string EmergencyLastName { get; set; }

        [Display(Name = "Emergency Contact Middle Name")]
        public string EmergencyMiddleName { get; set; }

        [Display(Name = "Emergency Contact Suffix")]
        public string EmergencySuffix { get; set; }

        [Required]
        [Display(Name = "Emergency Contact Number")]
        public string EmergencyContactNumber { get; set; }

        [Required]
        [Display(Name = "Relationship to Teacher")]
        public string Relationship { get; set; }
    }
}
