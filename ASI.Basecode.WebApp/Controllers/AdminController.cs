using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
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
    }
}


