using ASI.Basecode.Webapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for student-related actions and dashboard views.
    /// </summary>
    public class StudentController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudentController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public StudentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the student dashboard.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new StudentDashboardViewModel();

            viewModel.RecentlyGradedTasks = new List<StudentDashboardViewModel.TaskItem>();
            viewModel.ToBeGradedTasks = new List<StudentDashboardViewModel.TaskItem>();

            return View(viewModel);
        }

        /// <summary>
        /// Displays the student's courses.
        /// </summary>
        /// <returns>The courses view.</returns>
        [HttpGet]
        public IActionResult Courses()
        {
            var viewModel = new StudentCoursesViewModel
            {
                UserName = "First Name",
                Courses = new List<StudentCoursesViewModel.CourseItem>()
            };
            return View(viewModel);
        }

        /// <summary>
        /// Displays details for a specific course.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>The course details view.</returns>
        [HttpGet]
        public IActionResult CourseDetails(string courseId)
        {
            var viewModel = new StudentCourseDetailsViewModel
            {
                CourseId = courseId ?? "default",
                OverallGPA = 0,
                CompletedTasks = 0,
                TotalTasks = 0,
                PendingTasks = 0,
                Activities = new List<StudentCourseDetailsViewModel.ActivityItem>(),
                Appeals = new List<StudentCourseDetailsViewModel.AppealItem>(),
                Feedbacks = new List<StudentCourseDetailsViewModel.FeedbackItem>()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Gets the course title by its identifier.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>The course title.</returns>
        private string GetCourseTitleById(string courseId)
        {
            return courseId switch
            {
                "cs101" => "Introduction to Computer Science",
                "math201" => "Discrete Mathematics",
                "eng102" => "Technical Writing",
                "php41" => "Free Elective - PHP",
                _ => "Course Title"
            };
        }

        /// <summary>
        /// Displays the student's reports.
        /// </summary>
        /// <returns>The reports view.</returns>
        [HttpGet]
        public IActionResult Reports()
        {
            var viewModel = new StudentReportViewModel
            {
                Reports = new List<StudentReportViewModel.ReportItem>()
            };
            return View(viewModel);
        }

        /// <summary>
        /// Displays the student's notifications.
        /// </summary>
        /// <returns>The notifications view.</returns>
        [HttpGet]
        public IActionResult Notifications()
        {
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>()
            };
            if (!model.HasData)
                ViewBag.NoDataMessage = "No notifications available at the moment.";
            return View(model);
        }

        /// <summary>
        /// Returns the notification dropdown partial view.
        /// </summary>
        /// <returns>The notification dropdown partial view.</returns>
        [HttpGet]
        public PartialViewResult NotificationDropdown()
        {
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>()
            };
            return PartialView("_NotificationDropdown", model);
        }

        /// <summary>
        /// Gets the count of notifications.
        /// </summary>
        /// <returns>The notification count as JSON.</returns>
        [HttpGet]
        public IActionResult NotificationCount()
        {
            var count = 0;
            return Json(new { count });
        }
    }
}


