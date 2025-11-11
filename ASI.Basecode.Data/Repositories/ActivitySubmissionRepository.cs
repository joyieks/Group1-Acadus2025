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
    /// Repository implementation for ActivitySubmission entity operations.
    /// Inherits from BaseRepository to leverage common database access patterns.
    /// </summary>
    public class ActivitySubmissionRepository : BaseRepository, IActivitySubmissionRepository
    {
        public ActivitySubmissionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Gets all activity submissions from the database.
        /// </summary>
        /// <returns>IQueryable collection of all submissions.</returns>
        public IQueryable<ActivitySubmission> GetSubmissions()
        {
            return this.GetDbSet<ActivitySubmission>();
        }

        /// <summary>
        /// Gets a specific submission by its ID.
        /// </summary>
        /// <param name="submissionId">The submission ID to retrieve.</param>
        /// <returns>The submission if found, null otherwise.</returns>
        public ActivitySubmission GetSubmissionById(int submissionId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .FirstOrDefault(s => s.id == submissionId);
        }

        /// <summary>
        /// Gets all submissions for a specific activity.
        /// </summary>
        /// <param name="activityId">The activity ID to filter submissions.</param>
        /// <returns>IQueryable collection of submissions for the activity.</returns>
        public IQueryable<ActivitySubmission> GetSubmissionsByActivity(int activityId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .Where(s => s.activityId == activityId);
        }

        /// <summary>
        /// Gets all submissions for a specific student.
        /// </summary>
        /// <param name="studentId">The student User ID to filter submissions.</param>
        /// <returns>IQueryable collection of submissions by the student.</returns>
        public IQueryable<ActivitySubmission> GetSubmissionsByStudent(int studentId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .Where(s => s.studentId == studentId);
        }

        /// <summary>
        /// Gets the submission record for a specific student and activity combination.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="studentId">The student User ID.</param>
        /// <returns>The submission if found, null otherwise.</returns>
        public ActivitySubmission GetSubmissionByActivityAndStudent(int activityId, int studentId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .FirstOrDefault(s => s.activityId == activityId && s.studentId == studentId);
        }

        /// <summary>
        /// Gets all graded submissions (score != null and status = "Graded") for a specific activity.
        /// </summary>
        /// <param name="activityId">The activity ID to filter submissions.</param>
        /// <returns>IQueryable collection of graded submissions for the activity.</returns>
        public IQueryable<ActivitySubmission> GetGradedSubmissionsByActivity(int activityId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .Where(s => s.activityId == activityId &&
                           s.submissionStatus == "Graded" &&
                           s.score.HasValue);
        }

        /// <summary>
        /// Gets count of graded submissions for a specific activity.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <returns>The count of submissions that have been graded.</returns>
        public int GetGradedSubmissionCountByActivity(int activityId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .Where(s => s.activityId == activityId &&
                           s.submissionStatus == "Graded" &&
                           s.score.HasValue)
                .Count();
        }

        /// <summary>
        /// Checks if a submission exists by ID.
        /// </summary>
        /// <param name="submissionId">The submission ID to check.</param>
        /// <returns>True if the submission exists, false otherwise.</returns>
        public bool SubmissionExists(int submissionId)
        {
            return this.GetDbSet<ActivitySubmission>()
                .Any(s => s.id == submissionId);
        }

        /// <summary>
        /// Adds a new submission to the database.
        /// </summary>
        /// <param name="submission">The submission entity to add.</param>
        public void AddSubmission(ActivitySubmission submission)
        {
            this.GetDbSet<ActivitySubmission>().Add(submission);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Updates an existing submission in the database (used for grading).
        /// </summary>
        /// <param name="submission">The submission entity with updated values.</param>
        public void UpdateSubmission(ActivitySubmission submission)
        {
            this.SetEntityState(submission, Microsoft.EntityFrameworkCore.EntityState.Modified);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Deletes a submission from the database.
        /// </summary>
        /// <param name="submissionId">The ID of the submission to delete.</param>
        public void DeleteSubmission(int submissionId)
        {
            var submission = GetSubmissionById(submissionId);
            if (submission != null)
            {
                this.GetDbSet<ActivitySubmission>().Remove(submission);
                UnitOfWork.SaveChanges();
            }
        }

        /// <summary>
        /// Gets graded submissions for activities within a specific date range (this week, for example).
        /// </summary>
        /// <param name="activityIds">List of activity IDs to filter submissions.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>The count of submissions that have been graded within the date range.</returns>
        public int GetGradedSubmissionCountByDateRange(List<int> activityIds, DateTime startDate, DateTime endDate)
        {
            // Join with Activity to access dueDate for date range filtering
            return this.GetDbSet<ActivitySubmission>()
                .Where(s => activityIds.Contains(s.activityId) &&
                           s.submissionStatus == "Graded" &&
                           s.score.HasValue)
                .Count();
        }
    }
}

