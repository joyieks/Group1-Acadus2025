using System;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Represents an activity/assignment within a course.
    /// Can be quizzes, exams, assignments, projects, etc.
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// Primary key identifier for the activity.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Foreign key to Course table.
        /// Identifies which course this activity belongs to.
        /// </summary>
        public int courseId { get; set; }

        /// <summary>
        /// Title of the activity (e.g., Quiz 1, Midterm Exam, Assignment 3).
        /// </summary>
        public string activityTitle { get; set; }

        /// <summary>
        /// Detailed description of the activity and instructions.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Whether the activity has been archived.
        /// Default is false (not archived).
        /// </summary>
        public bool isArchived { get; set; } = false;

        /// <summary>
        /// Date and time when the activity was archived (nullable).
        /// Only populated if isArchived is true.
        /// </summary>
        public DateTime? archivedAt { get; set; }

        /// <summary>
        /// Due date and time for the activity (nullable).
        /// Can be null for activities without strict deadlines.
        /// </summary>
        public DateTime? dueDate { get; set; }

        /// <summary>
        /// Maximum possible points/score for this activity.
        /// </summary>
        public double maxScore { get; set; }
    }
}
