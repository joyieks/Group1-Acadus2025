using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for Activity entity operations.
    /// Provides methods to query and manage activities (assignments, quizzes, exams) in the database.
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Gets all activities from the database.
        /// </summary>
        /// <returns>IQueryable collection of all activities.</returns>
        IQueryable<Activity> GetActivities();

        /// <summary>
        /// Gets a specific activity by its ID.
        /// </summary>
        /// <param name="activityId">The activity ID to retrieve.</param>
        /// <returns>The activity if found, null otherwise.</returns>
        Activity GetActivityById(int activityId);

        /// <summary>
        /// Gets all activities for a specific course.
        /// </summary>
        /// <param name="courseId">The course ID to filter activities.</param>
        /// <returns>IQueryable collection of activities in the course.</returns>
        IQueryable<Activity> GetActivitiesByCourse(int courseId);

        /// <summary>
        /// Gets all activities for a specific course that are not archived.
        /// </summary>
        /// <param name="courseId">The course ID to filter activities.</param>
        /// <returns>IQueryable collection of non-archived activities in the course.</returns>
        IQueryable<Activity> GetActiveActivitiesByCourse(int courseId);

        /// <summary>
        /// Checks if an activity exists by ID.
        /// </summary>
        /// <param name="activityId">The activity ID to check.</param>
        /// <returns>True if the activity exists, false otherwise.</returns>
        bool ActivityExists(int activityId);

        /// <summary>
        /// Adds a new activity to the database.
        /// </summary>
        /// <param name="activity">The activity entity to add.</param>
        void AddActivity(Activity activity);

        /// <summary>
        /// Updates an existing activity in the database.
        /// </summary>
        /// <param name="activity">The activity entity with updated values.</param>
        void UpdateActivity(Activity activity);

        /// <summary>
        /// Deletes an activity from the database.
        /// </summary>
        /// <param name="activityId">The ID of the activity to delete.</param>
        void DeleteActivity(int activityId);

        /// <summary>
        /// Archives an activity by setting isArchived to true and archivedAt to current time.
        /// </summary>
        /// <param name="activityId">The ID of the activity to archive.</param>
        void ArchiveActivity(int activityId);

        /// <summary>
        /// Gets all activities due within a specific date range (this week, for example).
        /// </summary>
        /// <param name="courseIds">List of course IDs to filter activities.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>IQueryable collection of activities due within the date range.</returns>
        IQueryable<Activity> GetActivitiesByDueDateRange(List<int> courseIds, DateTime startDate, DateTime endDate);
    }
}
