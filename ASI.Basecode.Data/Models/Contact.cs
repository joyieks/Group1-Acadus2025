using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("contacts")]
    public class Contact : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("middle_name")]
        public string MiddleName { get; set; }

        [Column("suffix")]
        public string Suffix { get; set; }

        [Column("contact_number")]
        public string ContactNumber { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
