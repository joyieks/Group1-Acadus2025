using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for Activity data access operations with Supabase.
    /// Handles all CRUD and query operations related to course activities (assignments, quizzes, etc.).
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Gets all activities for a specific course.
        /// Supabase Query: SELECT * FROM activities WHERE courseId = courseId
        /// </summary>
        /// <param name="courseId">The course ID</param>
        /// <returns>List of activities in the course</returns>
        Task<List<Activity>> GetActivitiesByCourseAsync(int courseId);

        /// <summary>
        /// Gets all activities for multiple courses (batch query).
        /// Supabase Query: SELECT * FROM activities WHERE courseId IN (...)
        /// </summary>
        /// <param name="courseIds">List of course IDs</param>
        /// <returns>List of all activities across the specified courses</returns>
        Task<List<Activity>> GetActivitiesByCourseIdsAsync(List<int> courseIds);

        /// <summary>
        /// Gets a specific activity by its ID.
        /// Supabase Query: SELECT * FROM activities WHERE id = activityId LIMIT 1
        /// </summary>
        /// <param name="activityId">The activity ID</param>
        /// <returns>Activity object or null if not found</returns>
        Task<Activity> GetActivityByIdAsync(int activityId);

        /// <summary>
        /// Gets all non-archived activities for a course.
        /// Supabase Query: SELECT * FROM activities WHERE courseId = courseId AND isArchived = false
        /// </summary>
        /// <param name="courseId">The course ID</param>
        /// <returns>List of active (non-archived) activities</returns>
        Task<List<Activity>> GetActiveActivitiesByCourseAsync(int courseId);

        /// <summary>
        /// Gets activities that are due soon or overdue.
        /// Supabase Query: SELECT * FROM activities WHERE dueDate BETWEEN NOW() AND NOW() + INTERVAL
        /// </summary>
        /// <param name="courseId">The course ID</param>
        /// <param name="daysAhead">Number of days to look ahead</param>
        /// <returns>List of activities due within the specified timeframe</returns>
        Task<List<Activity>> GetUpcomingActivitiesByCourseAsync(int courseId, int daysAhead = 7);

        /// <summary>
        /// Creates a new activity.
        /// Supabase: INSERT INTO activities (...)
        /// </summary>
        /// <param name="activity">The activity object to create</param>
        /// <returns>The created activity with ID populated</returns>
        Task<Activity> CreateActivityAsync(Activity activity);

        /// <summary>
        /// Updates an existing activity.
        /// Supabase: UPDATE activities SET ... WHERE id = activityId
        /// </summary>
        /// <param name="activity">The activity object with updated values</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateActivityAsync(Activity activity);

        /// <summary>
        /// Soft-deletes an activity by setting isArchived = true.
        /// Supabase: UPDATE activities SET isArchived = true WHERE id = activityId
        /// </summary>
        /// <param name="activityId">The activity ID to archive</param>
        /// <returns>True if archive was successful</returns>
        Task<bool> ArchiveActivityAsync(int activityId);

        /// <summary>
        /// Hard-deletes an activity from the database.
        /// Supabase: DELETE FROM activities WHERE id = activityId
        /// </summary>
        /// <param name="activityId">The activity ID to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteActivityAsync(int activityId);
    }
}
