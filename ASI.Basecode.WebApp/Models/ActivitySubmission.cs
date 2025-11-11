namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents a student's grade/submission for an activity.
    /// Tracks the score a student received on a course activity.
    /// </summary>
    public class ActivitySubmission
    {
        /// <summary>
        /// Primary key identifier for the submission record.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Foreign key to Activity table.
        /// Identifies which activity this grade is for.
        /// </summary>
        public int activityId { get; set; }

        /// <summary>
        /// Foreign key to User table.
        /// Identifies the student who took/submitted the activity.
        /// </summary>
        public int studentId { get; set; }

        /// <summary>
        /// The score/points the student received (nullable).
        /// Null indicates the activity was not submitted or has not been graded yet.
        /// </summary>
        public double? score { get; set; }

        /// <summary>
        /// Status of the submission:
        /// - Submitted: Student has submitted the activity
        /// - Graded: Teacher has graded the submission
        /// - Late: Submission was submitted after the due date
        /// - Missing: Activity was not submitted
        /// Default is "Submitted".
        /// </summary>
        public string submissionStatus { get; set; } = "Submitted";

        /// <summary>
        /// Teacher's feedback/comments on the student's work (nullable).
        /// Provides constructive feedback for the student.
        /// </summary>
        public string feedback { get; set; }
    }
}
