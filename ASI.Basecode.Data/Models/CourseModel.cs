using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a course entity mapped to the 'courses' table in Supabase.
    /// </summary>
    [Table("courses")]
    public class CourseModel : BaseModel
    {
        /// <summary>
        /// Gets or sets the course ID.
        /// </summary>
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the course code.
        /// </summary>
        [Column("courseCode")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the course name.
        /// </summary>
        [Column("courseName")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the course description.
        /// </summary>
        [Column("courseDesc")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the number of credits for the course.
        /// </summary>
        [Column("credits")]
        public long Credits { get; set; }

        /// <summary>
        /// Gets or sets the course level (Undergraduate, Graduate, Doctorate).
        /// </summary>
        [Column("level")]
        public string Level { get; set; }

        /// <summary>
        /// Gets or sets the semester ID for the course.
        /// </summary>
        [Column("semesterId")]
        public long SemesterId { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity for the course.
        /// </summary>
        [Column("capacity")]
        public decimal MaxCapacity { get; set; }

        /// <summary>
        /// Gets or sets the instructor ID for the course.
        /// </summary>
        [Column("instructor")]
        public string TeacherId { get; set; }

        /// <summary>
        /// Gets or sets the course status (Active, Inactive).
        /// </summary>
        [Column("status")]
        public string Status { get; set; }
    }
}
