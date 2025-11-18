using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// ViewModel for the ViewCourse page containing course information and enrolled students
    /// </summary>
    public class ViewCourseViewModel
    {
        /// <summary>
        /// Gets or sets the course information
        /// </summary>
        public CourseModel Course { get; set; }

        /// <summary>
        /// Gets or sets the list of enrolled students
        /// </summary>
        public List<CourseEnrolledStudentViewModel> EnrolledStudents { get; set; } = new List<CourseEnrolledStudentViewModel>();
    }
}
