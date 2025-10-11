using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TeacherController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new TeacherDashboardViewModel
            {
                TotalActivities = 0,
                GradedActivities = 0,
                TotalCoursesHandled = 0
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Courses()
        {
            var courses = new List<TeacherCourseViewModel>
            {
                new TeacherCourseViewModel
                {
                    CourseCode = "91299 - ELPHP41",
                    CourseTitle = "FREE ELECTIVE - PHP",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    CourseCode = "91300 - CS101",
                    CourseTitle = "INTRODUCTION TO COMPUTER SCIENCE",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    CourseCode = "91301 - MATH201",
                    CourseTitle = "DISCRETE MATHEMATICS",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    CourseCode = "91302 - ENG102",
                    CourseTitle = "TECHNICAL WRITING",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#6EE7B7"
                },
                new TeacherCourseViewModel
                {
                    CourseCode = "91303 - DATA301",
                    CourseTitle = "DATA STRUCTURES",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#34D399"
                },
                new TeacherCourseViewModel
                {
                    CourseCode = "91304 - WEBDEV401",
                    CourseTitle = "WEB DEVELOPMENT",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#10B981"
                }
            };

            return View(courses.ToArray());
        }
    }
}


