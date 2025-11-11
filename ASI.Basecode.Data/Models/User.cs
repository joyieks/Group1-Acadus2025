using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a user account in the Acadus system.
    /// </summary>
    [Table("user")]
    public class User : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the user.
        /// </summary>
        [Key]
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        [Column("first_name")]
        public string firstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        [Column("last_name")]
        public string lastName { get; set; }

        /// <summary>
        /// User's middle name (nullable).
        /// </summary>
        [Column("middle_name")]
        public string middleName { get; set; }

        /// <summary>
        /// User's name suffix (e.g., Jr., Sr.) (nullable).
        /// </summary>
        [Column("suffix")]
        public string suffix { get; set; }

        /// <summary>
        /// User's email address.
        /// </summary>
        [Column("email")]
        public string email { get; set; }

        /// <summary>
        /// User's contact phone number.
        /// </summary>
        [Column("contact_number")]
        public string contactNumber { get; set; }

        /// <summary>
        /// Foreign key to Address table.
        /// </summary>
        [Column("address_id")]
        public int address { get; set; }

        /// <summary>
        /// Foreign key to EmergencyContact table.
        /// </summary>
        [Column("emergency_contact_id")]
        public int emergencyContact { get; set; }

        /// <summary>
        /// Unique university-issued ID (Student/Staff/Admin ID).
        /// Not a foreign key - just a unique identifier.
        /// </summary>
        [Column("user_type_id")]
        public int userTypeId { get; set; }

        /// <summary>
        /// Whether the user account is active (default: true).
        /// </summary>
        [Column("is_active")]
        public bool isActive { get; set; } = true;

        /// <summary>
        /// URL to user's profile picture (nullable).
        /// </summary>
        [Column("profile_picture_url")]
        public string profilePictureUrl { get; set; }
    }
}
