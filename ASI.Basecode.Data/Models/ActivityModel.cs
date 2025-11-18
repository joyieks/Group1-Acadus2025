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

        [Column("activityTitle")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("courseId")]
        public long CourseId { get; set; }

        [Column("maxScore")]
        public int maxScore { get; set; }

        [Column("dueDate")]
        public DateTime DueDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("isVisible")]
        public bool IsVisible { get; set; }

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }
    }
}
