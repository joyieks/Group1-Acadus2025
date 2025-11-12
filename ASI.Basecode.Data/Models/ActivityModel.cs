using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    [Table("activities")]
    public class ActivityModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("teacher_id")]
        public int TeacherId { get; set; }  // int4 in database

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("is_graded")]
        public bool IsGraded { get; set; }

        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; }

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }
    }
}
