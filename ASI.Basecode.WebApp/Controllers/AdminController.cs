using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;
using System.Security.Claims;
using ASI.Basecode.Data.Models;
using System;
using System.Linq;


namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IAdminService _adminService;

        public AdminController(IStudentService studentService, ITeacherService teacherService, ISupabaseAuthService supabaseAuthService, IAdminService adminService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _supabaseAuthService = supabaseAuthService;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var (totalStudents, totalInstructors, totalCourses) = await _adminService.GetDashboardStatisticsAsync();
            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View(new StudentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _studentService.CreateStudentAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Student {model.FirstName} {model.LastName} has been successfully created! A confirmation email has been sent to {model.Email}. The student must click the confirmation link in the email before they can log in. The temporary password has been logged for admin reference.";
                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create student. Please try again.");
                    return View(model);
                }
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating student: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return RedirectToAction("AddTeacher", "Teacher");
        }

        [HttpGet]
        public IActionResult Courses()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewUser(string id)
        {
            // TODO: Load user data by id
            return View();
        }

        [HttpGet]
        public IActionResult ViewTeacher(string id)
        {
            // TODO: Load teacher data by id
            return View();
        }

        [HttpGet]
        public IActionResult EditUser(string id)
        {
            // TODO: Load user data by id
            return View();
        }

        [HttpGet]
        public IActionResult RecentActivity()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PendingTasks()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewCourse(string id)
        {
            // TODO: Load course data by id
            return View();
        }

        [HttpGet]
        public IActionResult EditCourse(string id)
        {
            // TODO: Load course data by id
            return View();
        }

        [HttpGet]

        public IActionResult Teachers()
        {
            return RedirectToAction("Index", "Teacher");
        }

        [HttpGet]
        public IActionResult NotificationCount()
        {
            var count = 0; // sync with model above; replace with real count when available
            return Json(new { count });
        }

        /// <summary>
        /// Gets the admin database ID from the Supabase user ID
        /// </summary>
        private async Task<string> GetAdminIdFromSupabaseIdAsync(string supabaseUserId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                var userQuery = await client
                    .From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Get();

                var userRecord = userQuery?.Models?.FirstOrDefault();
                return userRecord?.Id.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting admin ID from Supabase ID: {ex.Message}");
                return null;
            }
        }
    }
}

