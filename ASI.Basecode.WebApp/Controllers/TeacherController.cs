using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using System;

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

        [HttpGet]
        public IActionResult Notifications()
        {
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>() // no seeded items
            };
            return View("~/Views/Shared/Notifications.cshtml", model);
        }

        [HttpGet]
        public PartialViewResult NotificationDropdown()
        {
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>()
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
        public IActionResult Profile()
        {
            // Use the same model as Home/Profile for consistency
            var model = new ASI.Basecode.WebApp.Models.StudentProfileViewModel
            {
                // ===== Basic Profile Information =====
                ProfileImageUrl = "/images/sample-profile.jpg",
                FullName = "Dr. Maria Santos",

                // ===== Personal Info =====
                StudentId = "T001",
                Status = "Active",
                FirstName = "Maria",
                MiddleName = "Reyes",
                LastName = "Santos",
                Suffix = "",
                DateOfBirth = "January 15, 1985",
                Gender = "Female",
                Course = "Faculty Member",
                YearLevel = "N/A",
                Department = "College of Computer Studies",
                EmailAddress = "maria.santos@example.com",
                PhoneNumber = "09171234568",

                // ===== Address Information =====
                HouseNumber = "456",
                Street = "Oak Avenue",
                Subdivision = "Green Valley Subdivision",
                Barangay = "Barangay San Antonio",
                City = "Quezon City",
                Province = "Metro Manila",
                ZipCode = "1101",

                // ===== Emergency Contact Information =====
                EmergencyFirstName = "Jose",
                EmergencyMiddleName = "Santos",
                EmergencyLastName = "Santos",
                EmergencySuffix = "",
                EmergencyContactNumber = "09181234568",
                EmergencyRelationship = "Spouse",

                // ===== Security Info =====
                PasswordLastUpdated = DateTime.Now.AddMonths(-1)
            };

            if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
            {
                model.ProfileImageUrl = uploadedUrl;
            }

            // If no profile data exists
            if (!model.HasData)
                ViewBag.NoDataMessage = "No profile data available. Please complete your profile information.";

            return View("~/Views/Shared/Profile.cshtml", model);
        }
    }
}


