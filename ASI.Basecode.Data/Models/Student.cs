using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("studentProfile")]
    public class Student : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("studentId")]
        public string StudentId { get; set; }  // References users.userTypeId (UUID)

        [Column("yearLevel")]
        public int? YearLevel { get; set; }

        [Column("programId")]  // Changed from "program" to "programId" (FK to programs table)
        public string ProgramId { get; set; }

        [Column("departmentId")]
        public string DepartmentId { get; set; }  // Changed from string to int to match departments table

        // ? RENAMED: Human-readable student display ID
        [Column("studentDisplayId")]
        public string StudentDisplayId { get; set; }  // STU-202511001

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
