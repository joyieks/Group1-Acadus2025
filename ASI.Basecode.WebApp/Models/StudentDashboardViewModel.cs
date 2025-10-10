using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model used by the student dashboard to display tasks and user information.
    /// </summary>
    public class StudentDashboardViewModel
    {
        /// <summary>Display name of the student.</summary>
        public string UserName { get; set; } = "First Name";

        /// <summary>List of recently graded tasks shown on the dashboard.</summary>
        public List<TaskItem> RecentlyGradedTasks { get; set; } = new();

        /// <summary>List of tasks that are pending grading.</summary>
        public List<TaskItem> ToBeGradedTasks { get; set; } = new();

        /// <summary>Represents a single task entry for dashboard lists.</summary>
        public class TaskItem
        {
            /// <summary>Title of the task or activity.</summary>
            public string Title { get; set; } = string.Empty;

            /// <summary>Action the user took related to this task (e.g., "Submitted").</summary>
            public string UserAction { get; set; } = string.Empty;

            /// <summary>Score or grade as shown on the UI (nullable if not graded).</summary>
            public string? Score { get; set; }

            /// <summary>Due date for the task if applicable.</summary>
            public DateTime? DueDate { get; set; }

            /// <summary>Priority label such as "High", "Normal" or "Low".</summary>
            public string? Priority { get; set; }

            /// <summary>Identifier of the student related to the task.</summary>
            public int? StudentId { get; set; }

            /// <summary>Identifier of the course related to the task.</summary>
            public int? CourseId { get; set; }
        }
    }
}
