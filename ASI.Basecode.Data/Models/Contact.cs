using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("emergencyContact")]
    public class Contact : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("firstName")]
        public string FirstName { get; set; }

        [Column("middleName")]
        public string MiddleName { get; set; }

        [Column("lastName")]
        public string LastName { get; set; }

        [Column("suffix")]
        public string Suffix { get; set; }

        [Column("contactNumber")]
        public string ContactNumber { get; set; }  // Keep as string - Supabase will handle conversion to numeric

        [Column("relationship")]
        public string Relationship { get; set; }  // Added - this column exists in the table
    }
}
