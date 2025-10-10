using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    // Maps to the 'courses' table in Supabase
    [Table("courses")]
    public class CourseModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        // Add other columns as needed, e.g.:
        [Column("code")]
        public string Code { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("max_capacity")]
        public int MaxCapacity { get; set; }

        [Column("enrolled_count")]
        public int EnrolledCount { get; set; }
    }
}
