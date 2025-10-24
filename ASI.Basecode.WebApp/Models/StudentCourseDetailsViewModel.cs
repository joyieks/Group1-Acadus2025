using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Models
{
    public class StudentCourseDetailsViewModel
    {
        public string UserName { get; set; } = "First Name";
        public string CourseTitle { get; set; } = "Course Title";
        public string CourseId { get; set; } = string.Empty;

        public double OverallGPA { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public List<AppealItem> Appeals { get; set; }
        public List<FeedbackItem> Feedbacks { get; set; }
        public List<ActivityItem> Activities { get; set; } = new();
        
        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string CurrentTab { get; set; } = "grades";

        public double GetCourseAverage()
        {
            var gradedActivities = Activities.Where(a => a.Status == "Graded" && int.TryParse(a.Score, out _)).ToList();
            if (!gradedActivities.Any()) return 0;
            var totalScore = gradedActivities.Sum(a => int.Parse(a.Score));
            return Math.Round((double)totalScore / gradedActivities.Count, 1);
        }

        public double GetCompletionPercentage()
        {
            return TotalTasks > 0 ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1) : 0;
        }

        public class ActivityItem
        {
            public string Term { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Score { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool CanAppeal { get; set; } = false;
            
            // Additional properties for the new mock data
            public string Title { get; set; } = string.Empty;
            public string DueDate { get; set; } = string.Empty;
        }

        public class AppealItem
        {
            public string ActivityName { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string Status { get; set; } = "Pending";
            public DateTime DateSubmitted { get; set; }
            
            // Additional properties for the new mock data
            public string Title { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class FeedbackItem
        {
            public string ActivityName { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string Instructor { get; set; } = string.Empty;
            public DateTime DateGiven { get; set; }
            
            // Additional properties for the new mock data
            public string Title { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }
    }
}
