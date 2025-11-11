using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents an academic department in the institution.
    /// </summary>
    public class Department : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the department.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Full name of the department (e.g., Computer Science, Mathematics).
        /// </summary>
        public string departmentName { get; set; }

        /// <summary>
        /// Short code for the department (e.g., CS, MATH, BUS).
        /// </summary>
        public string departmentCode { get; set; }
    }
}
