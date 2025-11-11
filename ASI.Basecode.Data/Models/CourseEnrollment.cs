using System;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a student's enrollment in a course.
    /// Junction table linking students (users) to courses.
    /// </summary>
    public class CourseEnrollment : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the enrollment record.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Foreign key to Course table.
        /// Identifies which course the student is enrolled in.
        /// </summary>
        public int courseId { get; set; }

        /// <summary>
        /// Foreign key to User table.
        /// Identifies the student enrolled in the course.
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// Enrollment status (Active or Dropped).
        /// Active: student is currently enrolled.
        /// Dropped: student has dropped the course.
        /// Default is "Active".
        /// </summary>
        public string enrollmentStatus { get; set; } = "Active";
    }
}
