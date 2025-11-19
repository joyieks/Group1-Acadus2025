using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
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
        public List<FeedbackItem> Feedbacks { get; set; }
        public List<ActivityItem> Activities { get; set; } = new();

        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string CurrentTab { get; set; } = "grades";

        public double ConvertPercentageToGPA(double percentage)
        {
            if (percentage >= 98) return 1.00;
            if (percentage >= 95) return 1.25;
            if (percentage >= 92) return 1.50;
            if (percentage >= 89) return 1.75;
            if (percentage >= 86) return 2.00;
            if (percentage >= 83) return 2.25;
            if (percentage >= 80) return 2.50;
            if (percentage >= 78) return 2.75;
            if (percentage >= 75) return 3.00;
            return 5.00;
        }

        public double GetCourseAverage()
        {
            var graded = Activities
                .Where(a => a.Status == "Graded" && double.TryParse(a.Percentage, out _))
                .Select(a => ConvertPercentageToGPA(double.Parse(a.Percentage)))
                .ToList();

            if (!graded.Any()) return 0;
            return Math.Round(graded.Average(), 2);
        }


        public double GetCompletionPercentage()
        {
            return TotalTasks > 0 ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1) : 0;
        }

        public class ActivityItem
        {
            public int ActivityId { get; set; }  // Added to identify the activity for submission
            public string Term { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string MaxScore { get; set; } = string.Empty;
            public string Score { get; set; } = string.Empty;
            public string Percentage { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool CanAppeal { get; set; } = false;
            public string Feedback { get; set; } = string.Empty; // Teacher's feedback

            // Additional properties for the new mock data
            public string Title { get; set; } = string.Empty;
            public string DueDate { get; set; } = string.Empty;
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
