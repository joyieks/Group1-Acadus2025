using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Models
{
    public class StudentCoursesViewModel
    {
        public string UserName { get; set; } = "First Name";
        public List<CourseItem> Courses { get; set; } = new();

        // Optional helper methods (for future filtering)
        public List<CourseItem> GetCoursesBySemester(string semester) =>
            Courses.Where(c => c.Semester == semester).ToList();

        public List<CourseItem> GetElectiveCourses() =>
            Courses.Where(c => c.IsElective).ToList();

        public double GetAverageProgress() =>
            Courses.Any() ? Courses.Average(c => c.Progress) : 0;

        public class CourseItem
        {
            public string CourseCode { get; set; } = string.Empty;
            public string CourseTitle { get; set; } = string.Empty;
            public string Semester { get; set; } = string.Empty;
            public string Color { get; set; } = "bg-cyan-400";
            public bool IsElective { get; set; } = false;
            public string? Description { get; set; }
            public int Credits { get; set; }
            public string? Professor { get; set; }
            public int Progress { get; set; } = 0;
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Department { get; set; }
            public List<string> Prerequisites { get; set; } = new();
        }
    }
}
