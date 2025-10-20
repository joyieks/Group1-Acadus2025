using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    [Table("addresses")]
    public class Address : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("house_number")]
        public string HouseNumber { get; set; }

        [Column("street_name")]
        public string StreetName { get; set; }

        [Column("subdivision")]
        public string Subdivision { get; set; }

        [Column("barangay")]
        public string Barangay { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("province")]
        public string Province { get; set; }

        [Column("zip_code")]
        public string ZipCode { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
