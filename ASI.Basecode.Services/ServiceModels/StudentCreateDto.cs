using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class StudentCreateDto
    {
        // Basic Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Suffix")]
        public string Suffix { get; set; }

        // Contact Information
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        // Address
        [Display(Name = "House Number")]
        public string HouseNumber { get; set; }

        [Display(Name = "Street Name")]
        public string StreetName { get; set; }

        [Display(Name = "Subdivision")]
        public string Subdivision { get; set; }

        [Required(ErrorMessage = "Barangay is required")]
        [Display(Name = "Barangay")]
        public string Barangay { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Province is required")]
        [Display(Name = "Province")]
        public string Province { get; set; }

        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        // Student Profile
        [Required(ErrorMessage = "Year level is required")]
        [Display(Name = "Year Level")]
        public decimal YearLevel { get; set; }

        [Required(ErrorMessage = "Program is required")]
        [Display(Name = "Program")]
        public string ProgramId { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }

        // Emergency Contact (Optional)
        [Display(Name = "Emergency Contact - First Name")]
        public string EmergencyContactFirstName { get; set; }

        [Display(Name = "Emergency Contact - Middle Name")]
        public string EmergencyContactMiddleName { get; set; }

        [Display(Name = "Emergency Contact - Last Name")]
        public string EmergencyContactLastName { get; set; }

        [Display(Name = "Emergency Contact - Suffix")]
        public string EmergencyContactSuffix { get; set; }

        [Display(Name = "Emergency Contact - Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string EmergencyContactNumber { get; set; }

        [Display(Name = "Emergency Contact - Relationship")]
        public string EmergencyContactRelationship { get; set; }
    }
}
