using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    [Table("semester")]
    public class SemesterModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("semesterName")]
        public string SemesterName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
