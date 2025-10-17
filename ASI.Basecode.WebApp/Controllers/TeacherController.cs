using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for teacher-related actions and dashboard statistics.
    /// </summary>
    public class TeacherController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public TeacherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the teacher dashboard with statistics.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            var client = AsiBasecodeDBContext.SupabaseClient;

            // TODO: Replace with actual teacher ID from authentication
            int teacherId = 1; // Example integer ID

            // Get total activities for the teacher
            var activitiesResponse = await client.From<ActivityModel>()
                .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                .Get();
            var activities = activitiesResponse.Models;
            int totalActivities = activities.Count;

            // Get graded activities for the teacher
            int gradedActivities = activities.Count(a => a.IsGraded);

            // Get total courses handled by the teacher
            var coursesResponse = await client.From<CourseModel>()
                .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                .Get();
            var courses = coursesResponse.Models;
            int totalCoursesHandled = courses.Count;

            // TODO: Implement calendar events retrieval if needed
            var calendarEvents = new List<string>();

            var model = new TeacherDashboardViewModel
            {
                TotalActivities = totalActivities,
                GradedActivities = gradedActivities,
                TotalCoursesHandled = totalCoursesHandled,
                CalendarEvents = calendarEvents
            };
            return View(model);
        }

        /// <summary>
        /// Displays the teacher's activities view.
        /// </summary>
        /// <returns>The activities view.</returns>
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

        /// <summary>
        /// Displays the teacher's grades view.
        /// </summary>
        /// <returns>The grades view.</returns>
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
    }
}



