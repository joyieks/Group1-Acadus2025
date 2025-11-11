using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for admin dashboard operations.
    /// Handles system-wide statistics and management data including:
    /// - User counts and statistics
    /// - Course and enrollment statistics
    /// - System health metrics
    /// </summary>
    public interface IAdminDashboardService
    {
        /// <summary>
        /// Gets the total count of active users in the system.
        /// </summary>
        /// <returns>Count of active users</returns>
        Task<int> GetTotalUsersAsync();

        /// <summary>
        /// Gets the total count of active courses.
        /// </summary>
        /// <returns>Count of active courses</returns>
        Task<int> GetTotalCoursesAsync();

        /// <summary>
        /// Gets the total count of students in the system.
        /// </summary>
        /// <returns>Count of student profiles</returns>
        Task<int> GetTotalStudentsAsync();

        /// <summary>
        /// Gets the total count of teachers in the system.
        /// </summary>
        /// <returns>Count of teacher profiles</returns>
        Task<int> GetTotalTeachersAsync();

        /// <summary>
        /// Gets the total count of activities in the system.
        /// </summary>
        /// <returns>Count of all activities</returns>
        Task<int> GetTotalActivitiesAsync();

        /// <summary>
        /// Gets all users with a specific role.
        /// </summary>
        /// <param name="roleId">The ID of the role</param>
        /// <returns>List of users with that role</returns>
        Task<List<User>> GetUsersByRoleAsync(int roleId);

        /// <summary>
        /// Gets enrollment statistics by course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>Enrollment count for the course</returns>
        Task<int> GetEnrollmentCountByCoursesAsync(int courseId);

        /// <summary>
        /// Gets a list of all courses with their enrollment counts.
        /// </summary>
        /// <returns>List of courses with enrollment info</returns>
        Task<List<Course>> GetAllCoursesWithEnrollmentAsync();

        /// <summary>
        /// Gets the average student enrollment across all courses.
        /// </summary>
        /// <returns>Average enrollment per course</returns>
        Task<double> GetAverageEnrollmentAsync();
    }
}
