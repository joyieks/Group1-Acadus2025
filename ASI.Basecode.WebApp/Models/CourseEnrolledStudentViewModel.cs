using System;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// ViewModel for displaying enrolled students in a course on the ViewCourse page
    /// </summary>
    public class CourseEnrolledStudentViewModel
    {
        /// <summary>
        /// Gets or sets the student's display ID (e.g., STU-202511001)
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// Gets or sets the student's full name (FirstName + LastName concatenated)
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the enrollment status (Active, Dropped, Completed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the Supabase Auth ID (for actions)
        /// </summary>
        public string StudentId { get; set; }
    }
}
