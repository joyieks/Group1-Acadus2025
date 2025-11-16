using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("teacher_addresses")]
    public class TeacherAddress : BaseModel
    {
        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Column("address_id")]
        public int AddressId { get; set; }

        [Column("address_type")]
        public string AddressType { get; set; } = "current";

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [Reference(typeof(Teacher))]
        public Teacher Teacher { get; set; }

        [Reference(typeof(Address))]
        public Address Address { get; set; }
    }
}
