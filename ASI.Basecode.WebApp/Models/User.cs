namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents a user account in the Acadus system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key identifier for the user.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        public string firstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        public string lastName { get; set; }

        /// <summary>
        /// User's middle name (nullable).
        /// </summary>
        public string middleName { get; set; }

        /// <summary>
        /// User's name suffix (e.g., Jr., Sr.) (nullable).
        /// </summary>
        public string suffix { get; set; }

        /// <summary>
        /// User's email address.
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// User's contact phone number.
        /// </summary>
        public string contactNumber { get; set; }

        /// <summary>
        /// Foreign key to Address table.
        /// </summary>
        public int address { get; set; }

        /// <summary>
        /// Foreign key to EmergencyContact table.
        /// </summary>
        public int emergencyContact { get; set; }

        /// <summary>
        /// Unique university-issued ID (Student/Staff/Admin ID).
        /// Not a foreign key - just a unique identifier.
        /// </summary>
        public int userTypeId { get; set; }

        /// <summary>
        /// Whether the user account is active (default: true).
        /// </summary>
        public bool isActive { get; set; } = true;

        /// <summary>
        /// URL to user's profile picture (nullable).
        /// </summary>
        public string profilePictureUrl { get; set; }
    }
}
