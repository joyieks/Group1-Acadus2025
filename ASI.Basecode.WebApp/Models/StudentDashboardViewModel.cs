using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public class StudentDashboardViewModel
    {
        public string UserName { get; set; } = "First Name";
        public List<TaskItem> RecentlyGradedTasks { get; set; } = new();
        public List<TaskItem> ToBeGradedTasks { get; set; } = new();

        public class TaskItem
        {
            public string Title { get; set; } = string.Empty;
            public string UserAction { get; set; } = string.Empty;
            public string? Score { get; set; }
            public DateTime? DueDate { get; set; }
            public string? Priority { get; set; }
            public int? StudentId { get; set; }
            public int? CourseId { get; set; }
        }
    }
}
