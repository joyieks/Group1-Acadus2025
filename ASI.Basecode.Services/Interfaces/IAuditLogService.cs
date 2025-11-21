using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for audit logging functionality.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Logs an activity to the audit_logs table.
        /// </summary>
        /// <param name="userId">The user's userTypeId (UUID).</param>
        /// <param name="userRole">The user's role (e.g., "Teacher", "Admin").</param>
        /// <param name="userName">The user's full name for display.</param>
        /// <param name="actionType">The type of action (e.g., "CREATE_ACTIVITY", "GRADE_STUDENT").</param>
        /// <param name="actionDescription">Human-readable description of the action.</param>
        /// <param name="courseId">Optional course ID related to the action.</param>
        /// <param name="courseCode">Optional course code for display.</param>
        /// <param name="courseName">Optional course name for display.</param>
        /// <param name="studentId">Optional student's userTypeId related to the action.</param>
        /// <param name="studentName">Optional student's full name for display.</param>
        /// <param name="activityId">Optional activity ID related to the action.</param>
        /// <param name="activityTitle">Optional activity title for display.</param>
        /// <param name="details">Optional JSON string with additional structured data.</param>
        /// <param name="metadata">Optional free-form metadata.</param>
        Task LogActivityAsync(
            string userId,
            string userRole,
            string userName,
            string actionType,
            string actionDescription,
            long? courseId = null,
            string courseCode = null,
            string courseName = null,
            string studentId = null,
            string studentName = null,
            int? activityId = null,
            string activityTitle = null,
            string details = null,
            string metadata = null);

        /// <summary>
        /// Gets recent audit logs for a specific user.
        /// </summary>
        /// <param name="userId">The user's userTypeId.</param>
        /// <param name="limit">Maximum number of logs to retrieve (default: 10).</param>
        /// <returns>List of audit log entries.</returns>
        Task<List<ASI.Basecode.Data.Models.AuditLogModel>> GetRecentLogsByUserAsync(string userId, int limit = 10);

        /// <summary>
        /// Gets recent audit logs for a specific course.
        /// </summary>
        /// <param name="courseId">The course ID.</param>
        /// <param name="limit">Maximum number of logs to retrieve (default: 10).</param>
        /// <returns>List of audit log entries.</returns>
        Task<List<ASI.Basecode.Data.Models.AuditLogModel>> GetRecentLogsByCourseAsync(long courseId, int limit = 10);

        /// <summary>
        /// Gets all recent audit logs (for Admin dashboard).
        /// </summary>
        /// <param name="limit">Maximum number of logs to retrieve (default: 10).</param>
        /// <returns>List of all audit log entries ordered by most recent.</returns>
        Task<List<ASI.Basecode.Data.Models.AuditLogModel>> GetAllRecentActivitiesAsync(int limit = 10);

        /// <summary>
        /// Gets recent audit logs filtered by user role.
        /// </summary>
        /// <param name="userRole">The role to filter by (e.g., "Admin", "Teacher", "Student").</param>
        /// <param name="limit">Maximum number of logs to retrieve (default: 10).</param>
        /// <returns>List of audit log entries for the specified role.</returns>
        Task<List<ASI.Basecode.Data.Models.AuditLogModel>> GetRecentActivitiesByRoleAsync(string userRole, int limit = 10);
    }
}





