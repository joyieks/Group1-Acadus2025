using ASI.Basecode.Webapp.Models;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using static ASI.Basecode.WebApp.Models.StudentCourseDetailsViewModel;
using static ASI.Basecode.WebApp.Models.StudentDashboardViewModel;

namespace ASI.Basecode.WebApp.Controllers
{
    public class StudentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new StudentDashboardViewModel();

            
            viewModel.RecentlyGradedTasks = new List<TaskItem>();
            viewModel.ToBeGradedTasks = new List<TaskItem>();

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

        public IActionResult CourseDetails(string? courseId)
        {
            var viewModel = new StudentCourseDetailsViewModel
            {
                CourseId = courseId ?? "default",
                //CourseTitle = GetCourseTitleById(),
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

        private string GetCourseTitleById(string? courseId)
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


