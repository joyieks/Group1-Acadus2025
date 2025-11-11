using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for teacher dashboard operations with Supabase.
    /// Handles retrieval of statistics for the teacher dashboard including:
    /// - Total courses handled
    /// - Total activities assigned
    /// - Graded activities count
    /// </summary>
    public interface ITeacherDashboardService
    {
        /// <summary>
        /// Gets complete dashboard statistics for a teacher in one call.
        /// This method aggregates all statistics needed for the dashboard view.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id)</param>
        /// <returns>DashboardStatistics containing all statistics</returns>
        Task<DashboardStatistics> GetDashboardStatisticsAsync(int teacherId);

        /// <summary>
        /// Gets the total number of active courses handled by the teacher.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher</param>
        /// <returns>Count of active courses where instructor = teacherId</returns>
        Task<int> GetTotalCoursesAsync(int teacherId);

        /// <summary>
        /// Gets the total number of activities created across all teacher's courses.
        /// Only counts non-archived activities.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher</param>
        /// <returns>Count of activities in courses taught by this teacher</returns>
        Task<int> GetTotalActivitiesAsync(int teacherId);

        /// <summary>
        /// Gets the count of activities that have at least one graded submission.
        /// An activity is considered "graded" if it has submissions with status = "Graded".
        /// </summary>
        /// <param name="teacherId">The ID of the teacher</param>
        /// <returns>Count of distinct activities with graded submissions</returns>
        Task<int> GetGradedActivitiesAsync(int teacherId);
    }
}
