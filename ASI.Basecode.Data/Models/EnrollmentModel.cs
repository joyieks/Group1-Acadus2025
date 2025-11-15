using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a student enrollment in a course.
    /// </summary>
    [Table("course_enrollment")]
    public class EnrollmentModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("student_id")]
        public string StudentId { get; set; }  // Changed from Guid to long

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("created_at")]
        public DateTime EnrolledAt { get; set; }

        [Column("enrollmentStatus")]
        public string Status { get; set; } // "active", "dropped", "completed"

        [Column("dropped_at")]
        public DateTime? DroppedAt { get; set; }
    }
}
