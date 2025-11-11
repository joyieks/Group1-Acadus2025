using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a role in the Acadus system (Student, Teacher, Admin).
    /// This is a simple lookup table (1NF compliant).
    /// Actual user-to-role assignments are stored in UserRole junction table.
    /// </summary>
    [Table("role")]
    public class Role : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the role.
        /// </summary>
        [Key]
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// Name of the role (Student, Teacher, Admin).
        /// </summary>
        [Column("role_name")]
        public string roleName { get; set; }

        /// <summary>
        /// Description of what this role does.
        /// </summary>
        [Column("description")]
        public string description { get; set; }
    }
}
