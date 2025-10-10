using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject IConfiguration to access Supabase settings
        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Initialize Supabase client
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            var client = AsiBasecodeDBContext.SupabaseClient;

            // Fetch all users from Supabase (users table is present)
            var usersResponse = await client.From<SupabaseUser>().Get();
            var users = usersResponse.Models;

            // If you have a Role property, filter by role. Otherwise, just count all users.
            ViewBag.StudentCount = users.Count;
            ViewBag.InstructorCount = 0; // Not available in SupabaseUser
            ViewBag.CourseCount = 0; // No courses table in Supabase

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
            return View();
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View();
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
    }
}


