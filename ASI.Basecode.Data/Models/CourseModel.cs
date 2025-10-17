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
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the course name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the course code.
        /// </summary>
        [Column("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the teacher ID for the course.
        /// </summary>
        [Column("teacher_id")]
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Gets or sets the course status.
        /// </summary>
        [Column("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity for the course.
        /// </summary>
        [Column("max_capacity")]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Gets or sets the enrolled count for the course.
        /// </summary>
        [Column("enrolled_count")]
        public int EnrolledCount { get; set; }

        /// <summary>
        /// Gets or sets the creation date for the course.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
