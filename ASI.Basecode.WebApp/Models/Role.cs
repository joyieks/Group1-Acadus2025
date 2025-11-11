namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents a role in the Acadus system (Student, Teacher, Admin).
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Primary key identifier for the role.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Name of the role (Student, Teacher, Admin).
        /// </summary>
        public string roleName { get; set; }

        /// <summary>
        /// Foreign key to the profile that corresponds to this role.
        /// Indicates which profile table/row to use.
        /// </summary>
        public int profileId { get; set; }

        /// <summary>
        /// Description of what this role does.
        /// </summary>
        public string description { get; set; }
    }
}
