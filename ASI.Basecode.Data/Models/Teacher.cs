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

        // ? Changed from int? to string to store department name directly
        [Column("departmentId")]
        public string DepartmentId { get; set; }  // Now stores text like "College of Computer Studies (CCS)"

        // ? NEW: Human-readable teacher display ID
        [Column("teacherDisplayId")]
        public string TeacherDisplayId { get; set; }  // FAC-202511001

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
