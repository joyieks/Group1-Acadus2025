using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents administrator-specific profile information.
    /// Each user can have at most one admin profile (enforced by UNIQUE constraint on userId).
    /// </summary>
    [Table("admin_profile")]
    public class AdminProfile : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the admin profile.
        /// </summary>
        [Key]
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// Foreign key to User table (UNIQUE).
        /// Links this profile to exactly one user.
        /// </summary>
        [Column("user_id")]
        public int userId { get; set; }

        /// <summary>
        /// Navigation property to User.
        /// Optional - only loaded if explicitly included in query.
        /// </summary>
        [ForeignKey("userId")]
        public virtual User User { get; set; }
    }
}

