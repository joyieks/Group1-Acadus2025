namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents student-specific profile information.
    /// </summary>
    public class StudentProfile
    {
        /// <summary>
        /// Primary key identifier for the student profile.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Student's current year level (1, 2, 3, or 4).
        /// </summary>
        public int yearLevel { get; set; }

        /// <summary>
        /// Foreign key to Program table.
        /// Indicates the academic program the student is enrolled in.
        /// </summary>
        public int programId { get; set; }

        /// <summary>
        /// Foreign key to Department table.
        /// Indicates the department offering the student's program.
        /// </summary>
        public int departmentId { get; set; }
    }
}
