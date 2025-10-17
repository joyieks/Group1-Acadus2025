using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model for the Teacher dashboard, containing statistics and activity data.
    /// This model is populated by the backend and provided to the UI for rendering dashboard information.
    /// </summary>
    public class TeacherDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of activities assigned to the teacher.
        /// Populated by backend logic in TeacherController.
        /// </summary>
        public int? TotalActivities { get; set; }

        /// <summary>
        /// Gets or sets the number of graded activities by the teacher.
        /// Populated by backend logic in TeacherController.
        /// </summary>
        public int? GradedActivities { get; set; }

        /// <summary>
        /// Gets or sets the total number of courses handled by the teacher.
        /// Populated by backend logic in TeacherController.
        /// </summary>
        public int? TotalCoursesHandled { get; set; }

        /// <summary>
        /// Gets or sets the calendar events for the teacher dashboard.
        /// Populated by backend logic in TeacherController. UI can display these events in the dashboard calendar section.
        /// </summary>
        public List<string> CalendarEvents { get; set; }
    }
}
