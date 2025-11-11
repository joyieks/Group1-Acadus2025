using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for student dashboard operations.
    /// Handles retrieval of data for the student dashboard including:
    /// - Enrolled courses
    /// - Activity submissions and grades
    /// - Course-specific information
    /// </summary>
    public interface IStudentDashboardService
    {
        /// <summary>
        /// Gets all courses a student is enrolled in.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>List of courses student is enrolled in</returns>
        Task<List<Course>> GetEnrolledCoursesAsync(int studentId);

        /// <summary>
        /// Gets all activities for enrolled courses that are not archived.
        /// Filters to only show pending/upcoming activities.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>List of pending activities across all enrolled courses</returns>
        Task<List<Activity>> GetPendingActivitiesAsync(int studentId);

        /// <summary>
        /// Gets all submissions by the student with related activity details.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>List of all submissions by this student</returns>
        Task<List<ActivitySubmission>> GetStudentSubmissionsAsync(int studentId);

        /// <summary>
        /// Gets the average score of all graded submissions for a student.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>Average score or 0 if no graded submissions</returns>
        Task<double> GetStudentAverageScoreAsync(int studentId);

        /// <summary>
        /// Gets all submissions by a student in a specific course.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of submissions for this course</returns>
        Task<List<ActivitySubmission>> GetCourseSubmissionsAsync(int studentId, int courseId);
    }
}
