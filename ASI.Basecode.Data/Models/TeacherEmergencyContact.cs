using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("teacher_emergency_contacts")]
    public class TeacherEmergencyContact : BaseModel
    {
        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Column("contact_id")]
        public int ContactId { get; set; }

        [Column("relationship")]
        public string Relationship { get; set; }

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [Reference(typeof(Teacher))]
        public Teacher Teacher { get; set; }

        [Reference(typeof(Contact))]
        public Contact Contact { get; set; }
    }
}
