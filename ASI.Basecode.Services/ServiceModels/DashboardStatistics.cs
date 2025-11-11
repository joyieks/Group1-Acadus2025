namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Data Transfer Object containing teacher dashboard statistics.
    /// </summary>
    public class DashboardStatistics
    {
        /// <summary>
        /// Gets or sets the total number of active courses handled by the teacher.
        /// </summary>
        public int TotalCoursesHandled { get; set; }

        /// <summary>
        /// Gets or sets the total number of non-archived activities across all teacher's courses.
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// Gets or sets the number of activities that have at least one graded submission.
        /// </summary>
        public int GradedActivities { get; set; }
    }
}
