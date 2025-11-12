using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("users")]
    public class SupabaseUserNew : BaseModel
    {
    [PrimaryKey("id", false)]
        public int Id { get; set; }

    [Column("firstName")]
public string FirstName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("lastName")]
        public string LastName { get; set; }

     [Column("middleName")]
     public string MiddleName { get; set; }

  [Column("suffix")]
        public string Suffix { get; set; }

        [Column("contactNumber")]
        public string ContactNumber { get; set; }

        [Column("address")]
        public string Address { get; set; }

    [Column("emergencyContact")]
    public string EmergencyContact { get; set; }

        [Column("userTypeId")]
        public string UserTypeId { get; set; }

      [Column("isActive")]
 public bool IsActive { get; set; }

        [Column("profilePictureUrl")]
        public string ProfilePictureUrl { get; set; }
    }
}
