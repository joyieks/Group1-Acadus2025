using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for Activity entity operations.
    /// Handles activity/assignment management in the Acadus system.
    /// Activities belong to courses and have submissions from students.
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Gets all activities (including archived).
        /// </summary>
        /// <returns>List of all activities</returns>
        Task<List<Activity>> GetAllActivitiesAsync();

        /// <summary>
        /// Gets a specific activity by its ID.
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <returns>Activity record or null if not found</returns>
        Task<Activity> GetActivityByIdAsync(int activityId);

        /// <summary>
        /// Gets all non-archived activities for a specific course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of active activities in the course</returns>
        Task<List<Activity>> GetActivitiesByCourseAsync(int courseId);

        /// <summary>
        /// Gets all activities for a course (including archived).
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of all activities (active and archived) in the course</returns>
        Task<List<Activity>> GetAllActivitiesByCourseAsync(int courseId);

        /// <summary>
        /// Gets all activities created by a specific instructor across all their courses.
        /// Joins activities with courses where instructor = teacherId.
        /// </summary>
        /// <param name="teacherId">The ID of the instructor (User.id)</param>
        /// <returns>List of activities created by this teacher</returns>
        Task<List<Activity>> GetActivitiesByInstructorAsync(int teacherId);

        /// <summary>
        /// Gets the count of non-archived activities for a course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>Count of active activities</returns>
        Task<int> GetActivityCountByCourseAsync(int courseId);

        /// <summary>
        /// Creates a new activity.
        /// </summary>
        /// <param name="activity">The Activity object to insert</param>
        /// <returns>The created Activity with ID populated</returns>
        Task<Activity> CreateActivityAsync(Activity activity);

        /// <summary>
        /// Updates an existing activity.
        /// </summary>
        /// <param name="activity">The Activity object with updated values</param>
        /// <returns>The updated Activity</returns>
        Task<Activity> UpdateActivityAsync(Activity activity);

        /// <summary>
        /// Archives an activity (soft delete).
        /// Sets isArchived = true and archived_at = current timestamp.
        /// </summary>
        /// <param name="activityId">The ID of the activity to archive</param>
        /// <returns>True if successful, false if not found</returns>
        Task<bool> ArchiveActivityAsync(int activityId);

        /// <summary>
        /// Gets all archived activities for a course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of archived activities</returns>
        Task<List<Activity>> GetArchivedActivitiesByCourseAsync(int courseId);

        /// <summary>
        /// Gets activities with pending or upcoming due dates.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of activities where dueDate is in the future</returns>
        Task<List<Activity>> GetUpcomingActivitiesByCourseAsync(int courseId);
    }
}
