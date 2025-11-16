using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("departments")]
    public class Department : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("departmentName")]
        public string DepartmentName { get; set; }

        [Column("departmentCode")]
        public string DepartmentCode { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
