using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents feedback given by a teacher on a student's activity.
    /// </summary>
    [Table("feedbacks")]
    public class FeedbackModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("grade_id")]
        public int GradeId { get; set; }

        [Column("teacher_id")]
        public int TeacherId { get; set; }  // int4 in database

        [Column("student_id")]
        public long StudentId { get; set; }  // int8 in database

        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Column("feedback_text")]
        public string FeedbackText { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
