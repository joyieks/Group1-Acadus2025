using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for admin-related actions and dashboard management.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the admin dashboard with statistics.
        /// </summary>
        /// <returns>The dashboard view.</returns>
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
            return View();
        }

        /// <summary>
        /// Displays the add teacher view.
        /// </summary>
        /// <returns>The add teacher view.</returns>
        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View();
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
    }
}


