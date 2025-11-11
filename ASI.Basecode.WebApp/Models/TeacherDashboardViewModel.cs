using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model for the Teacher dashboard, containing statistics and activity data.
    /// All statistics are scoped to the current week.
    /// </summary>
    public class TeacherDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of activities assigned to the teacher (this week).
        /// </summary>
        public int? TotalActivities { get; set; }

        /// <summary>
        /// Gets or sets the number of graded activities by the teacher (this week).
        /// </summary>
        public int? GradedActivities { get; set; }

        /// <summary>
        /// Gets or sets the total number of courses handled by the teacher.
        /// </summary>
        public int? TotalCoursesHandled { get; set; }

        /// <summary>
        /// Gets or sets the start date of the current week (Monday).
        /// </summary>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the current week (Sunday).
        /// </summary>
        public DateTime WeekEndDate { get; set; }

        /// <summary>
        /// Gets or sets the week display text (e.g., "Nov 11 - Nov 17, 2025").
        /// </summary>
        public string WeekDisplayText { get; set; }
    }
}

