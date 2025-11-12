using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("user_roles")]
    public class UserRole : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("userId")]
        public string UserId { get; set; }

        [Column("roleId")]
        public int RoleId { get; set; }  // Changed from string to int - now references roles.id
    }
}
