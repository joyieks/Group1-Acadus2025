namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents a semester/term in the academic calendar.
    /// </summary>
    public class Semester
    {
        /// <summary>
        /// Primary key identifier for the semester.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Name of the semester (e.g., Fall 2025, Spring 2026, Summer 2026).
        /// </summary>
        public string semesterName { get; set; }
    }
}
