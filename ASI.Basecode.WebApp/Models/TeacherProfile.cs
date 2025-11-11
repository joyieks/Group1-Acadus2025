namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents teacher-specific profile information.
    /// </summary>
    public class TeacherProfile
    {
        /// <summary>
        /// Primary key identifier for the teacher profile.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Foreign key to Department table.
        /// Indicates which department the teacher belongs to.
        /// </summary>
        public int departmentId { get; set; }
    }
}
