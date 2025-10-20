using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for admin-related actions and dashboard management.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;

        public AdminController(IStudentService studentService, ITeacherService teacherService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        /// <summary>
        /// Displays the users management view.
        /// </summary>
        /// <returns>The users view.</returns>
        [HttpGet]
        public IActionResult Users()
        {
            return View();
        }

        /// <summary>
        /// Displays the add student view.
        /// </summary>
        /// <returns>The add student view.</returns>
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

        /// <summary>
        /// Displays the add teacher view.
        /// </summary>
        /// <returns>The add teacher view.</returns>
        [HttpGet]
        public IActionResult AddTeacher()
        {
            return RedirectToAction("AddTeacher", "Teacher");
        }

        /// <summary>
        /// Displays the courses management view.
        /// </summary>
        /// <returns>The courses view.</returns>
        [HttpGet]
        public IActionResult Courses()
        {
            return View();
        }

        /// <summary>
        /// Displays the view for a specific user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The view user view.</returns>
        [HttpGet]
        public IActionResult ViewUser(string id)
        {
            // TODO: Load user data by id
            return View();
        }

        /// <summary>
        /// Displays the edit user view.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The edit user view.</returns>
        [HttpGet]
        public IActionResult EditUser(string id)
        {
            // TODO: Load user data by id
            return View();
        }

        /// <summary>
        /// Displays the recent activity view.
        /// </summary>
        /// <returns>The recent activity view.</returns>
        [HttpGet]
        public IActionResult RecentActivity()
        {
            return View();
        }

        /// <summary>
        /// Displays the pending tasks view.
        /// </summary>
        /// <returns>The pending tasks view.</returns>
        [HttpGet]
        public IActionResult PendingTasks()
        {
            return View();
        }

        /// <summary>
        /// Displays the edit profile view.
        /// </summary>
        /// <returns>The edit profile view.</returns>
        [HttpGet]
        public IActionResult EditProfile()
        {
            return View();
        }

        /// <summary>
        /// Displays the add course view.
        /// </summary>
        /// <returns>The add course view.</returns>
        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        /// <summary>
        /// Displays the view for a specific course.
        /// </summary>
        /// <param name="id">The course identifier.</param>
        /// <returns>The view course view.</returns>
        [HttpGet]
        public IActionResult ViewCourse(string id)
        {
            // TODO: Load course data by id
            return View();
        }

        /// <summary>
        /// Displays the edit course view.
        /// </summary>
        /// <param name="id">The course identifier.</param>
        /// <returns>The edit course view.</returns>
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
    }
}


