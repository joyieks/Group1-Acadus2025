using System;
using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a course offered by the institution.
    /// </summary>
    public class Course : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the course.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Course code (e.g., 91299, CS101).
        /// </summary>
        public string courseCode { get; set; }

        /// <summary>
        /// Course name/title (e.g., Free Elective - PHP).
        /// </summary>
        public string courseName { get; set; }

        /// <summary>
        /// Detailed course description.
        /// </summary>
        public string courseDesc { get; set; }

        /// <summary>
        /// Number of credit hours for the course.
        /// </summary>
        public int credits { get; set; }

        /// <summary>
        /// Foreign key to Semester table.
        /// Indicates which semester the course is offered.
        /// </summary>
        public int semesterId { get; set; }

        /// <summary>
        /// Maximum number of students allowed in the course.
        /// </summary>
        public int capacity { get; set; }

        /// <summary>
        /// Foreign key to User table (User.id of the instructor).
        /// The teacher/instructor who teaches this course.
        /// </summary>
        public int instructor { get; set; }

        /// <summary>
        /// Course status (Active or Archived).
        /// Default is "Active".
        /// </summary>
        public string status { get; set; } = "Active";
    }
}
