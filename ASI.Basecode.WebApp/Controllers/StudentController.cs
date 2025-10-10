using ASI.Basecode.Webapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _configuration;

        public StudentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new StudentDashboardViewModel
            {
                RecentlyGradedTasks = new List<StudentDashboardViewModel.TaskItem>(),
                ToBeGradedTasks = new List<StudentDashboardViewModel.TaskItem>()
            };
            return View(viewModel);
        }

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

        [HttpGet]
        public IActionResult Reports()
        {
            var viewModel = new StudentReportViewModel
            {
                Reports = new List<StudentReportViewModel.ReportItem>()
            };
            return View(viewModel);
        }

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
            var count = 0;
            return Json(new { count });
        }
    }
}


