using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model for the Teacher dashboard, containing statistics and activity data.
    /// </summary>
    public class TeacherDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of activities assigned to the teacher.
        /// </summary>
        public int? TotalActivities { get; set; }

        /// <summary>
        /// Gets or sets the number of graded activities by the teacher.
        /// </summary>
        public int? GradedActivities { get; set; }

        /// <summary>
        /// Gets or sets the total number of courses handled by the teacher.
        /// </summary>
        public int? TotalCoursesHandled { get; set; }
    }
}
