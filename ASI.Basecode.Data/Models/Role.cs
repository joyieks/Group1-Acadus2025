using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("roles")]
    public class Role : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

   [Column("roleName")]
        public string RoleName { get; set; }

        [Column("created_at")]
      public DateTime CreatedAt { get; set; }

        [Column("roleProfile")]
        public string RoleProfile { get; set; }
    }
}
