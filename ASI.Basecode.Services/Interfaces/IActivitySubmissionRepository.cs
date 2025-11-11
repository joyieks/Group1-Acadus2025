using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for ActivitySubmission entity operations.
    /// Handles student submissions and grades for activities in the Acadus system.
    /// Tracks submission status and scores for each student's activity submission.
    /// </summary>
    public interface IActivitySubmissionRepository
    {
        /// <summary>
        /// Gets all activity submissions (across all activities and students).
        /// </summary>
        /// <returns>List of all submissions</returns>
        Task<List<ActivitySubmission>> GetAllSubmissionsAsync();

        /// <summary>
        /// Gets a specific submission by its ID.
        /// </summary>
        /// <param name="submissionId">The ID of the submission</param>
        /// <returns>ActivitySubmission record or null if not found</returns>
        Task<ActivitySubmission> GetSubmissionByIdAsync(int submissionId);

        /// <summary>
        /// Gets all submissions for a specific activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <returns>List of submissions for this activity</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByActivityAsync(int activityId);

        /// <summary>
        /// Gets all submissions by a specific student.
        /// Joins with activity_submission where studentId matches.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>List of all submissions by this student</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByStudentAsync(int studentId);

        /// <summary>
        /// Gets all submissions for a specific student in a specific course.
        /// Requires joining with activities and courses.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of submissions for this student in this course</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByStudentAndCourseAsync(int studentId, int courseId);

        /// <summary>
        /// Gets a specific student's submission for a specific activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <param name="studentId">The ID of the student</param>
        /// <returns>ActivitySubmission or null if not found</returns>
        Task<ActivitySubmission> GetSubmissionByActivityAndStudentAsync(int activityId, int studentId);

        /// <summary>
        /// Gets the count of submissions for an activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <returns>Count of submissions</returns>
        Task<int> GetSubmissionCountByActivityAsync(int activityId);

        /// <summary>
        /// Gets the count of graded submissions for an activity.
        /// Where submissionStatus = "Graded" or "Submitted" with score assigned.
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <returns>Count of graded submissions</returns>
        Task<int> GetGradedSubmissionCountByActivityAsync(int activityId);

        /// <summary>
        /// Creates a new submission.
        /// </summary>
        /// <param name="submission">The ActivitySubmission object to insert</param>
        /// <returns>The created ActivitySubmission with ID populated</returns>
        Task<ActivitySubmission> CreateSubmissionAsync(ActivitySubmission submission);

        /// <summary>
        /// Updates an existing submission (e.g., grading a submission).
        /// </summary>
        /// <param name="submission">The ActivitySubmission object with updated values</param>
        /// <returns>The updated ActivitySubmission</returns>
        Task<ActivitySubmission> UpdateSubmissionAsync(ActivitySubmission submission);

        /// <summary>
        /// Gets all ungraded submissions for a specific activity.
        /// Where submissionStatus != "Graded".
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <returns>List of ungraded submissions</returns>
        Task<List<ActivitySubmission>> GetUngradedSubmissionsByActivityAsync(int activityId);

        /// <summary>
        /// Gets average score for all submissions of an activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity</param>
        /// <returns>Average score or 0 if no submissions</returns>
        Task<double> GetAverageScoreByActivityAsync(int activityId);

        /// <summary>
        /// Gets average score for a student across all their submissions.
        /// </summary>
        /// <param name="studentId">The ID of the student</param>
        /// <returns>Average score or 0 if no submissions</returns>
        Task<double> GetAverageScoreByStudentAsync(int studentId);
    }
}
