using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static ASI.Basecode.WebApp.Models.StudentCourseDetailsViewModel;
using static ASI.Basecode.WebApp.Models.StudentDashboardViewModel;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;

        public StudentController(ISupabaseAuthService supabaseAuthService)
        {
            _supabaseAuthService = supabaseAuthService;
        }
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
            var courses = new List<TeacherCourseViewModel>
            {
                new TeacherCourseViewModel
                {
                    Id = 1,
                    CourseCode = "IT101",
                    CourseTitle = "Introduction to Information Technology",
                    SemesterInfo = "1st Semester 2025",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    Id = 2,
                    CourseCode = "PROG201",
                    CourseTitle = "Object-Oriented Programming in Java",
                    SemesterInfo = "1st Semester 2025",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    Id = 3,
                    CourseCode = "ELEC102",
                    CourseTitle = "Digital Photography and Media Editing",
                    SemesterInfo = "1st Semester 2025",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    Id = 4,
                    CourseCode = "NET302",
                    CourseTitle = "Computer Networks and Security",
                    SemesterInfo = "2nd Semester 2025",
                    CardColor = "#6EE7B7"
                }
            };

            return View(courses.ToArray());
        }

        public IActionResult CourseDetails(string courseId, string tab = "grades", int page = 1)
        {
            // Get course details based on courseId
            var courseData = GetCourseDataById(courseId);
            
            const int pageSize = 10;
            
            // Get the appropriate data based on tab
            var allActivities = courseData.Activities;
            var allAppeals = courseData.Appeals;
            var allFeedbacks = courseData.Feedbacks;
            
            // Calculate pagination
            var totalItems = tab switch
            {
                "appeals" => allAppeals.Count,
                "feedback" => allFeedbacks.Count,
                _ => allActivities.Count
            };
            
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var currentPage = Math.Max(1, Math.Min(page, totalPages));
            var skip = (currentPage - 1) * pageSize;
            
            // Get paginated data
            var paginatedActivities = allActivities.Skip(skip).Take(pageSize).ToList();
            var paginatedAppeals = allAppeals.Skip(skip).Take(pageSize).ToList();
            var paginatedFeedbacks = allFeedbacks.Skip(skip).Take(pageSize).ToList();
            
            var viewModel = new StudentCourseDetailsViewModel
            {
                CourseId = courseId ?? "default",
                CourseTitle = courseData.CourseTitle,
                OverallGPA = courseData.OverallGPA,
                CompletedTasks = courseData.CompletedTasks,
                TotalTasks = courseData.TotalTasks,
                PendingTasks = courseData.PendingTasks,
                Activities = paginatedActivities,
                Appeals = paginatedAppeals,
                Feedbacks = paginatedFeedbacks,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                CurrentTab = tab
            };

            return View(viewModel);
        }

        private (string CourseTitle, double OverallGPA, int CompletedTasks, int TotalTasks, int PendingTasks, 
                List<StudentCourseDetailsViewModel.ActivityItem> Activities,
                List<StudentCourseDetailsViewModel.AppealItem> Appeals,
                List<StudentCourseDetailsViewModel.FeedbackItem> Feedbacks) GetCourseDataById(string courseId)
        {
            return courseId switch
            {
                "1" => ( // IT101 - Introduction to Information Technology
                    CourseTitle: "Introduction to Information Technology",
                    OverallGPA:3.2,
                    CompletedTasks:8,
                    TotalTasks:12,
                    PendingTasks:4,
                    Activities: new List<StudentCourseDetailsViewModel.ActivityItem>
                    {
                        //testing pagination
                        new() { Title = "Database Design Assignment", DueDate = "2025-10-15", Status = "Completed", Score = "85" },
                        new() { Title = "System Analysis Quiz", DueDate = "2025-10-20", Status = "Completed", Score = "88" },
                        new() { Title = "Network Security Project", DueDate = "2025-11-01", Status = "Pending", Score = "0" },
                        new() { Title = "Final Exam", DueDate = "2025-12-15", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" },
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" }
                    },
                    Appeals: new List<StudentCourseDetailsViewModel.AppealItem>
                    {
                        new() { Title = "Quiz2 Grade Appeal", Date = "2025-09-15", Status = "Approved", Description = "Request for grade review" }
                    },
                    Feedbacks: new List<StudentCourseDetailsViewModel.FeedbackItem>
                    {
                        new() { Title = "Assignment1 Feedback", Date = "2025-09-10", Content = "Excellent work on database design. Consider improving query optimization." }
                    }
                ),
                "2" => ( // PROG201 - Object-Oriented Programming in Java
                    CourseTitle: "Object-Oriented Programming in Java",
                    OverallGPA:3.5,
                    CompletedTasks:10,
                    TotalTasks:15,
                    PendingTasks:5,
                    Activities: new List<StudentCourseDetailsViewModel.ActivityItem>
                    {
                        new() { Title = "Java Basics Lab", DueDate = "2025-10-10", Status = "Completed", Score = "92" },
                        new() { Title = "OOP Principles Assignment", DueDate = "2025-10-25", Status = "Completed", Score = "89" },
                        new() { Title = "Data Structures Project", DueDate = "2025-11-10", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Programming Exam", DueDate = "2025-12-20", Status = "Pending", Score = "0" }
                    },
                    Appeals: new List<StudentCourseDetailsViewModel.AppealItem>(),
                    Feedbacks: new List<StudentCourseDetailsViewModel.FeedbackItem>
                    {
                        new() { Title = "Lab3 Feedback", Date = "2025-09-20", Content = "Good understanding of inheritance. Work on exception handling." }
                    }
                ),
                "3" => ( // ELEC102 - Digital Photography and Media Editing
                    CourseTitle: "Digital Photography and Media Editing",
                    OverallGPA:3.8,
                    CompletedTasks:6,
                    TotalTasks:8,
                    PendingTasks:2,
                    Activities: new List<StudentCourseDetailsViewModel.ActivityItem>
                    {
                        new() { Title = "Photography Portfolio", DueDate = "2025-10-30", Status = "Completed", Score = "96" },
                        new() { Title = "Photo Editing Project", DueDate = "2025-11-15", Status = "In Progress", Score = "0" },
                        new() { Title = "Final Creative Project", DueDate = "2025-12-10", Status = "Pending", Score = "0" }
                    },
                    Appeals: new List<StudentCourseDetailsViewModel.AppealItem>(),
                    Feedbacks: new List<StudentCourseDetailsViewModel.FeedbackItem>
                    {
                        new() { Title = "Portfolio Review", Date = "2025-10-05", Content = "Outstanding creative work! Excellent use of lighting and composition." }
                    }
                ),
                "4" => ( // NET302 - Computer Networks and Security
                    CourseTitle: "Computer Networks and Security",
                    OverallGPA:0.0, // Future semester course
                    CompletedTasks:0,
                    TotalTasks:10,
                    PendingTasks:10,
                    Activities: new List<StudentCourseDetailsViewModel.ActivityItem>
                    {
                        new() { Title = "Network Topology Lab", DueDate = "2026-02-15", Status = "Not Started", Score = "0" },
                        new() { Title = "Security Protocols Assignment", DueDate = "2026-03-01", Status = "Not Started", Score = "0" },
                        new() { Title = "Final Network Project", DueDate = "2026-05-30", Status = "Not Started", Score = "0" }
                    },
                    Appeals: new List<StudentCourseDetailsViewModel.AppealItem>(),
                    Feedbacks: new List<StudentCourseDetailsViewModel.FeedbackItem>()
                ),
                _ => (
                    CourseTitle: "Unknown Course",
                    OverallGPA:0.0,
                    CompletedTasks:0,
                    TotalTasks:0,
                    PendingTasks:0,
                    Activities: new List<StudentCourseDetailsViewModel.ActivityItem>(),
                    Appeals: new List<StudentCourseDetailsViewModel.AppealItem>(),
                    Feedbacks: new List<StudentCourseDetailsViewModel.FeedbackItem>()
                ),
            };

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


        // -------------------- Notifications Controller --------------------

        [HttpGet]
        public IActionResult Notifications()
        {
            var model = new NotificationsViewModel
            {


                Notifications = new List<NotificationsViewModel.NotificationItem>
        {
            new NotificationsViewModel.NotificationItem
            {
                Title = "New Message from Admin",
                Message = "Your account has been successfully verified.",
                Date = DateTime.Now.AddMinutes(-15),
                IsRead = false
            },
            new NotificationsViewModel.NotificationItem
            {
                Title = "System Maintenance",
                Message = "Scheduled maintenance on October 30, 2025, from 1:00 AM to 3:00 AM.",
                Date = DateTime.Now.AddHours(-2),
                IsRead = true
            },
            new NotificationsViewModel.NotificationItem
            {
                Title = "Grade Update",
                Message = "Your final grade for IT 331 (Database Systems) has been posted.",
                Date = DateTime.Now.AddDays(-1),
                IsRead = false
            }
        }

            };

            if (!model.HasData)
                ViewBag.NoDataMessage = "No notifications available at the moment.";

            return View("~/Views/Shared/Notifications.cshtml", model);
        }

        [HttpGet]
        public PartialViewResult NotificationDropdown()
        {
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>
        {
            new NotificationsViewModel.NotificationItem
            {
                Title = "New Assignment Posted",
                Message = "A new assignment is available in IT 335.",
                Date = DateTime.Now.AddHours(-3),
                IsRead = false
            },
            new NotificationsViewModel.NotificationItem
            {
                Title = "Reminder",
                Message = "Submit your project proposal before October 28.",
                Date = DateTime.Now.AddDays(-2),
                IsRead = true
            }
        }
            };
            return PartialView("_NotificationDropdown", model);
        }

        [HttpGet]
        public IActionResult NotificationCount()
        {

            var count = 0; 
            return Json(new { count });
        }

        // -------------------- Reports Controller --------------------
        public IActionResult Reports()
        {
            var viewModel = new StudentReportViewModel
            {
                Reports = new List<StudentReportViewModel.ReportItem>
        {
            new StudentReportViewModel.ReportItem
            {
                CourseCode = "IT 331",
                CourseTitle = "Database Systems",
                MidtermGrade = 89.5,
                FinalGrade = 91.0
            },
            new StudentReportViewModel.ReportItem
            {
                CourseCode = "IT 332",
                CourseTitle = "Systems Analysis and Design",
                MidtermGrade = 85.0,
                FinalGrade = 88.5
            },
            new StudentReportViewModel.ReportItem
            {
                CourseCode = "IT 333",
                CourseTitle = "Web Development",
                MidtermGrade = 92.0,
                FinalGrade = 95.0
            }
        }
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var supabaseUserId = HttpContext.Session.GetString("SupabaseUserId");
            if (string.IsNullOrWhiteSpace(supabaseUserId))
            {
                ViewBag.NoDataMessage = "Session expired. Please log in again.";
                return View(new StudentProfileViewModel());
            }

            var model = new StudentProfileViewModel();
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Load user info (all personal data is in users table)
                var user = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Single();

                if (user != null)
                {
                    model.FirstName = user.FirstName;
                    model.MiddleName = user.MiddleName;
                    model.LastName = user.LastName;
                    model.FullName = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    model.EmailAddress = user.Email;
                    
                    // Get studentProfile for academic info
                    var studentProfile = await client.From<Student>()
                      .Where(x => x.StudentId == supabaseUserId)
                    .Single();
                    
                  model.Department = studentProfile?.DepartmentId?.ToString() ?? "N/A";
                  model.Course = studentProfile?.ProgramId?.ToString() ?? "N/A";  // Fixed: use ProgramId
  model.YearLevel = studentProfile?.YearLevel?.ToString() ?? "N/A";
    model.Status = user.IsActive ? "Active" : "Inactive";
                }

               // Address (primary)
             if (user != null)
               {
             // Get studentProfile ID first
              var studentProfile = await client.From<Student>()
.Where(x => x.StudentId == supabaseUserId)
            .Single();
     
    var studentAddress = await client.From<StudentAddress>()
            .Where(sa => sa.StudentId == studentProfile.Id && sa.IsPrimary == true)  // Fixed: use studentProfile.Id (int)
            .Single();

    if (studentAddress != null)
     {
        var address = await client.From<Address>()
 .Where(a => a.Id == studentAddress.AddressId)
       .Single();
              if (address != null)
  {
       model.HouseNumber = address.HouseNumber;
      model.Street = address.StreetName;
   model.Subdivision = address.Subdivision;
          model.Barangay = address.Barangay;
     model.City = address.City;
      model.Province = address.Province;
        model.ZipCode = address.ZipCode;
           }
          }
             }

              // Emergency contact (primary)
          if (user != null)
     {
    var studentProfile = await client.From<Student>()
.Where(x => x.StudentId == supabaseUserId)
         .Single();
   
 var emergency = await client.From<StudentEmergencyContact>()
 .Where(ec => ec.StudentId == studentProfile.Id && ec.IsPrimary == true)  // Fixed: use studentProfile.Id (int)
        .Single();
           if (emergency != null)
    {
  var contact = await client.From<Contact>()
 .Where(c => c.Id == emergency.ContactId)
    .Single();
 if (contact != null)
      {
        model.EmergencyFirstName = contact.FirstName;
           model.EmergencyMiddleName = contact.MiddleName;
       model.EmergencyLastName = contact.LastName;
   model.EmergencySuffix = contact.Suffix;
  model.EmergencyContactNumber = contact.ContactNumber;
         model.EmergencyRelationship = emergency.Relationship;
        }
           }
      }

    // Profile image from Auth metadata (set by upload)
                model.ProfileImageUrl = await _supabaseAuthService.GetUserProfileImageUrlAsync(supabaseUserId);

                // If recent upload exists in this session, prefer it
                if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    model.ProfileImageUrl = uploadedUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading student profile: {ex.Message}");
                ViewBag.NoDataMessage = "Unable to load profile data at the moment.";
            }

            if (!model.HasData)
                ViewBag.NoDataMessage = "No profile data available.";

            return View(model);
        }
    }
}
