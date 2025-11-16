using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class StudentViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Suffix")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Year level is required.")]
        [Range(1, 4, ErrorMessage = "Year level must be between 1 and 4.")]
        [Display(Name = "Year Level")]
        public int YearLevel { get; set; }

        [Required(ErrorMessage = "Program is required.")]
        [Display(Name = "Program/Course")]
        public string Program { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public string Department { get; set; } = "Computer Science Department";

        [Display(Name = "House/Apt Number")]
        public string HouseNumber { get; set; }

        [Required(ErrorMessage = "Street name is required.")]
        [Display(Name = "Street Name")]
        public string StreetName { get; set; }

        [Display(Name = "Subdivision/Bldg.")]
        public string Subdivision { get; set; }

        [Required(ErrorMessage = "Barangay is required.")]
        [Display(Name = "Barangay")]
        public string Barangay { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Province is required.")]
        [Display(Name = "Province")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Zip code is required.")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Emergency contact first name is required.")]
        [Display(Name = "Emergency Contact First Name")]
        public string EmergencyFirstName { get; set; }

        [Required(ErrorMessage = "Emergency contact last name is required.")]
        [Display(Name = "Emergency Contact Last Name")]
        public string EmergencyLastName { get; set; }

        [Display(Name = "Emergency Contact Middle Name")]
        public string EmergencyMiddleName { get; set; }

        [Display(Name = "Emergency Contact Suffix")]
        public string EmergencySuffix { get; set; }

        [Required(ErrorMessage = "Emergency contact number is required.")]
        [Display(Name = "Emergency Contact Number")]
        public string EmergencyContactNumber { get; set; }

        [Required(ErrorMessage = "Relationship to student is required.")]
        [Display(Name = "Relationship to Student")]
        public string Relationship { get; set; }
    }
}
