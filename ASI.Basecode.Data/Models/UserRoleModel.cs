using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("user_roles")]
    public class UserRoleModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("userId")]
        public string UserId { get; set; }

        [Column("roleId")]
        public string RoleId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
