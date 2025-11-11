using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for ActivitySubmission entity operations.
    /// Provides methods to query and manage student activity submissions and grades.
    /// </summary>
    public interface IActivitySubmissionRepository
    {
        /// <summary>
        /// Gets all activity submissions from the database.
        /// </summary>
        /// <returns>IQueryable collection of all submissions.</returns>
        IQueryable<ActivitySubmission> GetSubmissions();

        /// <summary>
        /// Gets a specific submission by its ID.
        /// </summary>
        /// <param name="submissionId">The submission ID to retrieve.</param>
        /// <returns>The submission if found, null otherwise.</returns>
        ActivitySubmission GetSubmissionById(int submissionId);

        /// <summary>
        /// Gets all submissions for a specific activity.
        /// </summary>
        /// <param name="activityId">The activity ID to filter submissions.</param>
        /// <returns>IQueryable collection of submissions for the activity.</returns>
        IQueryable<ActivitySubmission> GetSubmissionsByActivity(int activityId);

        /// <summary>
        /// Gets all submissions for a specific student.
        /// </summary>
        /// <param name="studentId">The student User ID to filter submissions.</param>
        /// <returns>IQueryable collection of submissions by the student.</returns>
        IQueryable<ActivitySubmission> GetSubmissionsByStudent(int studentId);

        /// <summary>
        /// Gets the submission record for a specific student and activity combination.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="studentId">The student User ID.</param>
        /// <returns>The submission if found, null otherwise.</returns>
        ActivitySubmission GetSubmissionByActivityAndStudent(int activityId, int studentId);

        /// <summary>
        /// Gets all graded submissions (score != null and status = "Graded") for a specific activity.
        /// </summary>
        /// <param name="activityId">The activity ID to filter submissions.</param>
        /// <returns>IQueryable collection of graded submissions for the activity.</returns>
        IQueryable<ActivitySubmission> GetGradedSubmissionsByActivity(int activityId);

        /// <summary>
        /// Gets count of graded submissions for a specific activity.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <returns>The count of submissions that have been graded.</returns>
        int GetGradedSubmissionCountByActivity(int activityId);

        /// <summary>
        /// Checks if a submission exists by ID.
        /// </summary>
        /// <param name="submissionId">The submission ID to check.</param>
        /// <returns>True if the submission exists, false otherwise.</returns>
        bool SubmissionExists(int submissionId);

        /// <summary>
        /// Adds a new submission to the database.
        /// </summary>
        /// <param name="submission">The submission entity to add.</param>
        void AddSubmission(ActivitySubmission submission);

        /// <summary>
        /// Updates an existing submission in the database (used for grading).
        /// </summary>
        /// <param name="submission">The submission entity with updated values.</param>
        void UpdateSubmission(ActivitySubmission submission);

        /// <summary>
        /// Deletes a submission from the database.
        /// </summary>
        /// <param name="submissionId">The ID of the submission to delete.</param>
        void DeleteSubmission(int submissionId);

        /// <summary>
        /// Gets graded submissions for activities within a specific date range (this week, for example).
        /// </summary>
        /// <param name="activityIds">List of activity IDs to filter submissions.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>The count of submissions that have been graded within the date range.</returns>
        int GetGradedSubmissionCountByDateRange(List<int> activityIds, DateTime startDate, DateTime endDate);
    }
}
