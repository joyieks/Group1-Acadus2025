using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    [Table("grades")]
    public class GradeModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("student_id")]
        public long StudentId { get; set; }  // int8 in database

        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Column("grade")]
        public decimal Grade { get; set; }

        [Column("graded_at")]
        public DateTime GradedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("graded_by")]
        public int GradedBy { get; set; }  // int4 in database (teacher_id)
    }
}
