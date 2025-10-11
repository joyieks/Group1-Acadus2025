using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.Webapp.Models;
using System.Collections.Generic;
using System.Linq;

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
                    Id = 1,
                    CourseCode = "91299 - ELPHP41",
                    CourseTitle = "FREE ELECTIVE - PHP",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    Id = 2,
                    CourseCode = "91300 - CS101",
                    CourseTitle = "INTRODUCTION TO COMPUTER SCIENCE",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    Id = 3,
                    CourseCode = "91301 - MATH201",
                    CourseTitle = "DISCRETE MATHEMATICS",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    Id = 4,
                    CourseCode = "91302 - ENG102",
                    CourseTitle = "TECHNICAL WRITING",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#6EE7B7"
                },
                new TeacherCourseViewModel
                {
                    Id = 5,
                    CourseCode = "91303 - DATA301",
                    CourseTitle = "DATA STRUCTURES",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#34D399"
                },
                new TeacherCourseViewModel
                {
                    Id = 6,
                    CourseCode = "91304 - WEBDEV401",
                    CourseTitle = "WEB DEVELOPMENT",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#10B981"
                }
            };

            return View("Courses/Index", courses.ToArray());
        }

        [HttpGet]
        public IActionResult FullCourseView(int id)
        {
            // Placeholder: Find course by id from sample data
            var courses = new List<TeacherCourseViewModel>
            {
                new TeacherCourseViewModel
                {
                    Id = 1,
                    CourseCode = "91299 - ELPHP41",
                    CourseTitle = "FREE ELECTIVE - PHP",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    Id = 2,
                    CourseCode = "91300 - CS101",
                    CourseTitle = "INTRODUCTION TO COMPUTER SCIENCE",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    Id = 3,
                    CourseCode = "91301 - MATH201",
                    CourseTitle = "DISCRETE MATHEMATICS",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    Id = 4,
                    CourseCode = "91302 - ENG102",
                    CourseTitle = "TECHNICAL WRITING",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#6EE7B7"
                },
                new TeacherCourseViewModel
                {
                    Id = 5,
                    CourseCode = "91303 - DATA301",
                    CourseTitle = "DATA STRUCTURES",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#34D399"
                },
                new TeacherCourseViewModel
                {
                    Id = 6,
                    CourseCode = "91304 - WEBDEV401",
                    CourseTitle = "WEB DEVELOPMENT",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#10B981"
                }
            };
            var course = courses.FirstOrDefault(c => c.Id == id) ?? new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };
            return View("Courses/FullCourseView", course);
        }

        [HttpGet]
        public IActionResult CourseStudents(int id)
        {
            // Placeholder
            var course = new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };
            return View("Courses/CourseStudents", course);
        }

        [HttpGet]
        public IActionResult EditCourse(int id)
        {
            // Placeholder
            var course = new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };
            return View("Courses/EditCourse", course);
        }

        /// <summary>
        /// Displays the gradebook for a specific course.
        /// </summary>
        /// <param name="id">The course ID.</param>
        /// <returns>The gradebook view for the course.</returns>
        [HttpGet]
        public IActionResult Gradebook(int id)
        {
            // Fetch course data
            var courses = new List<TeacherCourseViewModel>
            {
                new TeacherCourseViewModel
                {
                    Id = 1,
                    CourseCode = "91299 - ELPHP41",
                    CourseTitle = "FREE ELECTIVE - PHP",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    Id = 2,
                    CourseCode = "91300 - CS101",
                    CourseTitle = "INTRODUCTION TO COMPUTER SCIENCE",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    Id = 3,
                    CourseCode = "91301 - MATH201",
                    CourseTitle = "DISCRETE MATHEMATICS",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    Id = 4,
                    CourseCode = "91302 - ENG102",
                    CourseTitle = "TECHNICAL WRITING",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#6EE7B7"
                },
                new TeacherCourseViewModel
                {
                    Id = 5,
                    CourseCode = "91303 - DATA301",
                    CourseTitle = "DATA STRUCTURES",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#34D399"
                },
                new TeacherCourseViewModel
                {
                    Id = 6,
                    CourseCode = "91304 - WEBDEV401",
                    CourseTitle = "WEB DEVELOPMENT",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#10B981"
                }
            };
            var course = courses.FirstOrDefault(c => c.Id == id) ?? new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };

            // Sample activities
            var activities = new List<string>
            {
                "Quiz 1", "Assignment 1", "Midterm Exam", "Quiz 2", "Assignment 2", "Final Project", "Participation", "Lab 1", "Lab 2", "Presentation", "Group Project"
            };

            // Sample student grades
            var studentGrades = new List<StudentGradeViewModel>
            {
                new StudentGradeViewModel
                {
                    IdNumber = "2021001",
                    LastName = "Doe",
                    FirstName = "John",
                    ActivityGrades = new Dictionary<string, double?>
                    {
                        ["Quiz 1"] = 85,
                        ["Assignment 1"] = 90,
                        ["Midterm Exam"] = 78,
                        ["Quiz 2"] = 92,
                        ["Assignment 2"] = 88,
                        ["Final Project"] = 95,
                        ["Participation"] = 100,
                        ["Lab 1"] = 89,
                        ["Lab 2"] = 91,
                        ["Presentation"] = 93,
                        ["Group Project"] = 87
                    }
                },
                new StudentGradeViewModel
                {
                    IdNumber = "2021002",
                    LastName = "Smith",
                    FirstName = "Jane",
                    ActivityGrades = new Dictionary<string, double?>
                    {
                        ["Quiz 1"] = 92,
                        ["Assignment 1"] = 85,
                        ["Midterm Exam"] = 88,
                        ["Quiz 2"] = 90,
                        ["Assignment 2"] = 95,
                        ["Final Project"] = 90,
                        ["Participation"] = 95,
                        ["Lab 1"] = 94,
                        ["Lab 2"] = 88,
                        ["Presentation"] = 96,
                        ["Group Project"] = 92
                    }
                },
                // Add more sample students
                new StudentGradeViewModel
                {
                    IdNumber = "2021003",
                    LastName = "Johnson",
                    FirstName = "Bob",
                    ActivityGrades = new Dictionary<string, double?>
                    {
                        ["Quiz 1"] = 78,
                        ["Assignment 1"] = 82,
                        ["Midterm Exam"] = 85,
                        ["Quiz 2"] = 80,
                        ["Assignment 2"] = 87,
                        ["Final Project"] = 88,
                        ["Participation"] = 90,
                        ["Lab 1"] = 83,
                        ["Lab 2"] = 79,
                        ["Presentation"] = 85,
                        ["Group Project"] = 81
                    }
                },
                new StudentGradeViewModel
                {
                    IdNumber = "2021004",
                    LastName = "Williams",
                    FirstName = "Alice",
                    ActivityGrades = new Dictionary<string, double?>
                    {
                        ["Quiz 1"] = 95,
                        ["Assignment 1"] = 98,
                        ["Midterm Exam"] = 92,
                        ["Quiz 2"] = 96,
                        ["Assignment 2"] = 94,
                        ["Final Project"] = 97,
                        ["Participation"] = 100,
                        ["Lab 1"] = 99,
                        ["Lab 2"] = 95,
                        ["Presentation"] = 98,
                        ["Group Project"] = 96
                    }
                },
                new StudentGradeViewModel
                {
                    IdNumber = "2021005",
                    LastName = "Brown",
                    FirstName = "Charlie",
                    ActivityGrades = new Dictionary<string, double?>
                    {
                        ["Quiz 1"] = 88,
                        ["Assignment 1"] = 85,
                        ["Midterm Exam"] = 80,
                        ["Quiz 2"] = 87,
                        ["Assignment 2"] = 90,
                        ["Final Project"] = 85,
                        ["Participation"] = 92,
                        ["Lab 1"] = 86,
                        ["Lab 2"] = 89,
                        ["Presentation"] = 91,
                        ["Group Project"] = 84
                    }
                }
            };

            // Calculate total grades
            foreach (var student in studentGrades)
            {
                var validGrades = student.ActivityGrades.Values.Where(g => g.HasValue).Select(g => g.Value);
                student.TotalGrade = validGrades.Any() ? validGrades.Average() : null;
            }

            var model = new GradebookViewModel
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                CourseTitle = course.CourseTitle,
                SemesterInfo = course.SemesterInfo,
                CardColor = course.CardColor,
                StudentGrades = studentGrades,
                Activities = activities
            };

            return View("Courses/Gradebook", model);
        }

        [HttpGet]
        public PartialViewResult NotificationDropdown()
        {
            var model = new ASI.Basecode.Webapp.Models.NotificationsViewModel
            {
                Notifications = new List<ASI.Basecode.Webapp.Models.NotificationsViewModel.NotificationItem>()
            };
            return PartialView("_NotificationDropdown", model);
        }

        [HttpGet]
        public IActionResult NotificationCount()
        {
            var count = 0; // sync with model above; replace with real count when available
            return Json(new { count });
        }

        [HttpGet]
        public IActionResult Notifications()
        {
            var model = new ASI.Basecode.Webapp.Models.NotificationsViewModel
            {
                Notifications = new List<ASI.Basecode.Webapp.Models.NotificationsViewModel.NotificationItem>() // no seeded items
            };

            if (!model.HasData)
                ViewBag.NoDataMessage = "No notifications available at the moment.";

            return View(model);
        }
    }
}


