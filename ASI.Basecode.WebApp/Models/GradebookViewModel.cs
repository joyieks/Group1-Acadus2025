using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// ViewModel for the Gradebook page, containing course information and a list of student grades.
    /// </summary>
    public class GradebookViewModel : TeacherCourseViewModel
    {
        /// <summary>
        /// Gets or sets the list of student grade records for the course.
        /// </summary>
        public List<StudentGradeViewModel> StudentGrades { get; set; } = new List<StudentGradeViewModel>();

        /// <summary>
        /// Gets or sets the list of activity names for the gradebook columns.
        /// </summary>
        public List<string> Activities { get; set; } = new List<string>();
    }

    /// <summary>
    /// ViewModel representing a student's grade record, including individual activity grades and total grade.
    /// </summary>
    public class StudentGradeViewModel
    {
        /// <summary>
        /// Gets or sets the student's ID number.
        /// </summary>
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dictionary of activity grades, where key is activity name and value is the grade.
        /// </summary>
        public Dictionary<string, double?> ActivityGrades { get; set; } = new Dictionary<string, double?>();

        /// <summary>
        /// Gets or sets the calculated total grade (average of activity grades).
        /// </summary>
        public double? TotalGrade { get; set; }
    }
}
