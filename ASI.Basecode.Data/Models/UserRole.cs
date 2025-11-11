using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Junction/Many-to-Many table linking users to their roles.
    /// Supports multiple roles per user (e.g., user can be both Student and Admin).
    /// </summary>
    [Table("user_role")]
    public class UserRole : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the user role assignment.
        /// </summary>
        [Key]
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// Foreign key to User table.
        /// Identifies which user has this role.
        /// </summary>
        [Column("user_id")]
        public int userId { get; set; }

        /// <summary>
        /// Foreign key to Role table.
        /// Identifies which role the user has.
        /// </summary>
        [Column("role_id")]
        public int roleId { get; set; }

        /// <summary>
        /// Navigation property to User.
        /// Optional - only loaded if explicitly included in query.
        /// </summary>
        [ForeignKey("userId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Navigation property to Role.
        /// Optional - only loaded if explicitly included in query.
        /// </summary>
        [ForeignKey("roleId")]
        public virtual Role Role { get; set; }
    }
}
