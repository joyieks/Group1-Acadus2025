using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("programs")]
    public class Program : BaseModel
    {
        [PrimaryKey("id", false)]
    public int Id { get; set; }

        [Column("programName")]
  public string ProgramName { get; set; }

  [Column("programCode")]
     public string ProgramCode { get; set; }

        [Column("departmentId")]
      public int? DepartmentId { get; set; }

        [Column("created_at")]
     public DateTime CreatedAt { get; set; }
    }
}
