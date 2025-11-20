using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Text.Json;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents an audit log entry for tracking teacher and admin activities.
    /// </summary>
    [Table("audit_logs")]
    public class AuditLogModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("userId")]
        public string UserId { get; set; }  // User's userTypeId (UUID)

        [Column("userRole")]
        public string UserRole { get; set; }  // "Teacher", "Admin", etc.

        [Column("userName")]
        public string UserName { get; set; }  // User's full name for display

        [Column("actionType")]
        public string ActionType { get; set; }  // "CREATE_ACTIVITY", "GRADE_STUDENT", etc.

        [Column("actionDescription")]
        public string ActionDescription { get; set; }  // Human-readable description

        [Column("courseId")]
        public long? CourseId { get; set; }

        [Column("courseCode")]
        public string CourseCode { get; set; }

        [Column("courseName")]
        public string CourseName { get; set; }

        [Column("studentId")]
        public string StudentId { get; set; }  // Student's userTypeId (UUID)

        [Column("studentName")]
        public string StudentName { get; set; }

        [Column("activityId")]
        public int? ActivityId { get; set; }

        [Column("activityTitle")]
        public string ActivityTitle { get; set; }

        [Column("details")]
        public string Details { get; set; }  // JSON string for additional data

        [Column("metadata")]
        public string Metadata { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}


