using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("teacherProfile")]
    public class Teacher : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("teacherId")]
        public string TeacherId { get; set; }  // References users.userTypeId

        [Column("departmentId")]
        public int? DepartmentId { get; set; }  // Changed from string to int to match departments table

        // ? NEW: Human-readable teacher display ID
        [Column("teacherDisplayId")]
        public string TeacherDisplayId { get; set; }  // FAC-202511001

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
