using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Activity entity operations.
    /// Inherits from BaseRepository to leverage common database access patterns.
    /// </summary>
    public class ActivityRepository : BaseRepository, IActivityRepository
    {
        public ActivityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Gets all activities from the database.
        /// </summary>
        /// <returns>IQueryable collection of all activities.</returns>
        public IQueryable<Activity> GetActivities()
        {
            return this.GetDbSet<Activity>();
        }

        /// <summary>
        /// Gets a specific activity by its ID.
        /// </summary>
        /// <param name="activityId">The activity ID to retrieve.</param>
        /// <returns>The activity if found, null otherwise.</returns>
        public Activity GetActivityById(int activityId)
        {
            return this.GetDbSet<Activity>()
                .FirstOrDefault(a => a.id == activityId);
        }

        /// <summary>
        /// Gets all activities for a specific course.
        /// </summary>
        /// <param name="courseId">The course ID to filter activities.</param>
        /// <returns>IQueryable collection of activities in the course.</returns>
        public IQueryable<Activity> GetActivitiesByCourse(int courseId)
        {
            return this.GetDbSet<Activity>()
                .Where(a => a.courseId == courseId);
        }

        /// <summary>
        /// Gets all activities for a specific course that are not archived.
        /// </summary>
        /// <param name="courseId">The course ID to filter activities.</param>
        /// <returns>IQueryable collection of non-archived activities in the course.</returns>
        public IQueryable<Activity> GetActiveActivitiesByCourse(int courseId)
        {
            return this.GetDbSet<Activity>()
                .Where(a => a.courseId == courseId && !a.isArchived);
        }

        /// <summary>
        /// Checks if an activity exists by ID.
        /// </summary>
        /// <param name="activityId">The activity ID to check.</param>
        /// <returns>True if the activity exists, false otherwise.</returns>
        public bool ActivityExists(int activityId)
        {
            return this.GetDbSet<Activity>()
                .Any(a => a.id == activityId);
        }

        /// <summary>
        /// Adds a new activity to the database.
        /// </summary>
        /// <param name="activity">The activity entity to add.</param>
        public void AddActivity(Activity activity)
        {
            this.GetDbSet<Activity>().Add(activity);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Updates an existing activity in the database.
        /// </summary>
        /// <param name="activity">The activity entity with updated values.</param>
        public void UpdateActivity(Activity activity)
        {
            this.SetEntityState(activity, Microsoft.EntityFrameworkCore.EntityState.Modified);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Deletes an activity from the database.
        /// </summary>
        /// <param name="activityId">The ID of the activity to delete.</param>
        public void DeleteActivity(int activityId)
        {
            var activity = GetActivityById(activityId);
            if (activity != null)
            {
                this.GetDbSet<Activity>().Remove(activity);
                UnitOfWork.SaveChanges();
            }
        }

        /// <summary>
        /// Archives an activity by setting isArchived to true and archivedAt to current time.
        /// </summary>
        /// <param name="activityId">The ID of the activity to archive.</param>
        public void ArchiveActivity(int activityId)
        {
            var activity = GetActivityById(activityId);
            if (activity != null)
            {
                activity.isArchived = true;
                activity.archivedAt = DateTime.Now;
                UpdateActivity(activity);
            }
        }

        /// <summary>
        /// Gets all activities due within a specific date range (this week, for example).
        /// </summary>
        /// <param name="courseIds">List of course IDs to filter activities.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>IQueryable collection of activities due within the date range.</returns>
        public IQueryable<Activity> GetActivitiesByDueDateRange(List<int> courseIds, DateTime startDate, DateTime endDate)
        {
            return this.GetDbSet<Activity>()
                .Where(a => courseIds.Contains(a.courseId) &&
                           !a.isArchived &&
                           a.dueDate.HasValue &&
                           a.dueDate >= startDate &&
                           a.dueDate <= endDate);
        }
    }
}

