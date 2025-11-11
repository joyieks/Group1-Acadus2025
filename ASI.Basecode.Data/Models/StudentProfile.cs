using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents student-specific profile information.
    /// Each user can have at most one student profile (enforced by UNIQUE constraint on userId).
    /// </summary>
    public class StudentProfile : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the student profile.
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
        /// Student's current year level (1, 2, 3, or 4).
        /// </summary>
        [Column("year_level")]
        public int yearLevel { get; set; }

        /// <summary>
        /// Foreign key to Program table.
        /// Indicates the academic program the student is enrolled in.
        /// </summary>
        [Column("program_id")]
        public int programId { get; set; }

        /// <summary>
        /// Foreign key to Department table.
        /// Indicates the department offering the student's program.
        /// </summary>
        [Column("department_id")]
        public int departmentId { get; set; }

        /// <summary>
        /// Navigation property to User.
        /// Optional - only loaded if explicitly included in query.
        /// </summary>
        [ForeignKey("userId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Navigation property to Program.
        /// Optional - only loaded if explicitly included in query.
        /// </summary>
        [ForeignKey("programId")]
        public virtual Program Program { get; set; }

        /// <summary>
        /// Navigation property to Department.
        /// Optional - only loaded if explicitly included in query.
        /// </summary>
        [ForeignKey("departmentId")]
        public virtual Department Department { get; set; }
    }
}

