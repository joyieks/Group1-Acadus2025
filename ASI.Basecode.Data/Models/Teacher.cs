using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("teachers")]
    public class Teacher : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("supabase_user_id")]
        public string SupabaseUserId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("middle_name")]
        public string MiddleName { get; set; }

        [Column("suffix")]
        public string Suffix { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("contact_number")]
        public string ContactNumber { get; set; }

        [Column("id_number")]
        public string IdNumber { get; set; }

        [Column("department")]
        public string Department { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
