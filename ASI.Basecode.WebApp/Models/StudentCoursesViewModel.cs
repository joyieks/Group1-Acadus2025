using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model representing the list of courses a student is enrolled in and helper methods.
    /// </summary>
    public class StudentCoursesViewModel
    {
        /// <summary>Display name of the student.</summary>
        public string UserName { get; set; } = "First Name";

        /// <summary>Collection of courses the student is currently enrolled in.</summary>
        public List<CourseItem> Courses { get; set; } = new();

        // Optional helper methods (for future filtering)
        public List<CourseItem> GetCoursesBySemester(string semester) =>
            Courses.Where(c => c.Semester == semester).ToList();

        public List<CourseItem> GetElectiveCourses() =>
            Courses.Where(c => c.IsElective).ToList();

        public double GetAverageProgress() =>
            Courses.Any() ? Courses.Average(c => c.Progress) : 0;

        /// <summary>Represents a single course entry in the list.</summary>
        public class CourseItem
        {
            /// <summary>Short course code (e.g., CS101).</summary>
            public string CourseCode { get; set; } = string.Empty;

            /// <summary>Full course title.</summary>
            public string CourseTitle { get; set; } = string.Empty;

            /// <summary>Semester label (e.g., "Fall 2025").</summary>
            public string Semester { get; set; } = string.Empty;

            /// <summary>Tailwind color class used in UI chips.</summary>
            public string Color { get; set; } = "bg-cyan-400";

            /// <summary>Whether the course is an elective.</summary>
            public bool IsElective { get; set; } = false;

            /// <summary>Optional course description.</summary>
            public string? Description { get; set; }

            /// <summary>Credit units for the course.</summary>
            public int Credits { get; set; }

            /// <summary>Professor or instructor name.</summary>
            public string? Professor { get; set; }

            /// <summary>Completion progress percentage.</summary>
            public int Progress { get; set; } = 0;

            /// <summary>Optional course start date.</summary>
            public DateTime? StartDate { get; set; }

            /// <summary>Optional course end date.</summary>
            public DateTime? EndDate { get; set; }

            /// <summary>Department offering the course.</summary>
            public string? Department { get; set; }

            /// <summary>List of prerequisites course codes.</summary>
            public List<string> Prerequisites { get; set; } = new();
        }
    }
}
