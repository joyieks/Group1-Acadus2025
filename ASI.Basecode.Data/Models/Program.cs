using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents an academic program/degree offered by the institution.
    /// </summary>
    public class Program : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the program.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Full name of the academic program (e.g., Bachelor of Science in Information Technology).
        /// </summary>
        public string programName { get; set; }

        /// <summary>
        /// Short code for the program (e.g., BSIT, BSCS, BSMATH).
        /// </summary>
        public string programCode { get; set; }

        /// <summary>
        /// Foreign key to Department table.
        /// Indicates which department offers this program.
        /// </summary>
        public int departmentId { get; set; }
    }
}
