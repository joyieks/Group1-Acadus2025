using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("student_addresses")]
    public class StudentAddress : BaseModel
    {
        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("address_id")]
        public int AddressId { get; set; }

        [Column("address_type")]
        public string AddressType { get; set; } = "current";

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [Reference(typeof(Student))]
        public Student Student { get; set; }

        [Reference(typeof(Address))]
        public Address Address { get; set; }
    }
}
