using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a user (student, teacher, or admin) in the system.
    /// </summary>
    [Table("users")]
    public class UserModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }  // Changed from Guid to long

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("middle_name")]
        public string MiddleName { get; set; }

        [Column("role")]
        public string Role { get; set; } // "student", "teacher", "admin"

        [Column("student_id")]
        public string StudentId { get; set; }

        [Column("program")]
        public string Program { get; set; }

        [Column("year_level")]
        public int? YearLevel { get; set; }

        [Column("contact_number")]
        public string ContactNumber { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
