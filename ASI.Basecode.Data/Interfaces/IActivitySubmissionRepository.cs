using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for ActivitySubmission (grades) data access operations with Supabase.
    /// Handles all CRUD and query operations related to student grades on activities.
    /// </summary>
    public interface IActivitySubmissionRepository
    {
        /// <summary>
        /// Gets all submissions for a specific activity.
        /// Supabase Query: SELECT * FROM activity_submissions WHERE activityId = activityId
        /// </summary>
        /// <param name="activityId">The activity ID</param>
        /// <returns>List of all submissions for the activity (all students)</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByActivityAsync(int activityId);

        /// <summary>
        /// Gets all submissions for multiple activities (batch query).
        /// Supabase Query: SELECT * FROM activity_submissions WHERE activityId IN (...)
        /// </summary>
        /// <param name="activityIds">List of activity IDs</param>
        /// <returns>List of all submissions across the specified activities</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByActivityIdsAsync(List<int> activityIds);

        /// <summary>
        /// Gets a specific submission by its ID.
        /// Supabase Query: SELECT * FROM activity_submissions WHERE id = submissionId LIMIT 1
        /// </summary>
        /// <param name="submissionId">The submission ID</param>
        /// <returns>ActivitySubmission object or null if not found</returns>
        Task<ActivitySubmission> GetSubmissionByIdAsync(int submissionId);

        /// <summary>
        /// Gets all submissions by a specific student in a specific course.
        /// Requires JOIN: activity_submissions WHERE studentId = studentId AND activityId IN (courseActivities)
        /// </summary>
        /// <param name="studentId">The student user ID</param>
        /// <param name="courseId">The course ID</param>
        /// <returns>List of submissions for the student in the course</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByStudentInCourseAsync(int studentId, int courseId);

        /// <summary>
        /// Gets a student's submission for a specific activity.
        /// Supabase Query: SELECT * FROM activity_submissions WHERE activityId = activityId AND studentId = studentId LIMIT 1
        /// </summary>
        /// <param name="studentId">The student user ID</param>
        /// <param name="activityId">The activity ID</param>
        /// <returns>ActivitySubmission object or null if not found</returns>
        Task<ActivitySubmission> GetSubmissionByStudentAndActivityAsync(int studentId, int activityId);

        /// <summary>
        /// Gets all graded submissions for an activity.
        /// Supabase Query: SELECT * FROM activity_submissions WHERE activityId = activityId AND submissionStatus = 'Graded'
        /// </summary>
        /// <param name="activityId">The activity ID</param>
        /// <returns>List of graded submissions only</returns>
        Task<List<ActivitySubmission>> GetGradedSubmissionsByActivityAsync(int activityId);

        /// <summary>
        /// Gets all submissions for an activity with a specific status.
        /// Supabase Query: SELECT * FROM activity_submissions WHERE activityId = activityId AND submissionStatus = status
        /// </summary>
        /// <param name="activityId">The activity ID</param>
        /// <param name="submissionStatus">The status to filter by (e.g., "Graded", "Submitted", "Missing")</param>
        /// <returns>List of submissions with the specified status</returns>
        Task<List<ActivitySubmission>> GetSubmissionsByActivityAndStatusAsync(int activityId, string submissionStatus);

        /// <summary>
        /// Creates a new submission (grades a student activity).
        /// Supabase: INSERT INTO activity_submissions (...)
        /// </summary>
        /// <param name="submission">The submission object to create</param>
        /// <returns>The created submission with ID populated</returns>
        Task<ActivitySubmission> CreateSubmissionAsync(ActivitySubmission submission);

        /// <summary>
        /// Updates an existing submission (updates a grade).
        /// Supabase: UPDATE activity_submissions SET ... WHERE id = submissionId
        /// </summary>
        /// <param name="submission">The submission object with updated values</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateSubmissionAsync(ActivitySubmission submission);

        /// <summary>
        /// Updates the score and status of a submission (grades activity).
        /// Supabase: UPDATE activity_submissions SET score = score, submissionStatus = 'Graded', feedback = feedback WHERE id = submissionId
        /// </summary>
        /// <param name="submissionId">The submission ID</param>
        /// <param name="score">The score given</param>
        /// <param name="feedback">Optional feedback for the student</param>
        /// <returns>True if update was successful</returns>
        Task<bool> GradeSubmissionAsync(int submissionId, double score, string feedback = null);

        /// <summary>
        /// Deletes a submission.
        /// Supabase: DELETE FROM activity_submissions WHERE id = submissionId
        /// </summary>
        /// <param name="submissionId">The submission ID to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteSubmissionAsync(int submissionId);

        /// <summary>
        /// Gets count of graded submissions for a specific activity.
        /// Supabase Query: SELECT COUNT(*) FROM activity_submissions WHERE activityId = activityId AND submissionStatus = 'Graded'
        /// </summary>
        /// <param name="activityId">The activity ID</param>
        /// <returns>Count of graded submissions</returns>
        Task<int> GetGradedSubmissionCountAsync(int activityId);
    }
}
