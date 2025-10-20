using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("student_emergency_contacts")]
    public class StudentEmergencyContact : BaseModel
    {
        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("contact_id")]
        public int ContactId { get; set; }

        [Column("relationship")]
        public string Relationship { get; set; }

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [Reference(typeof(Student))]
        public Student Student { get; set; }

        [Reference(typeof(Contact))]
        public Contact Contact { get; set; }
    }
}
