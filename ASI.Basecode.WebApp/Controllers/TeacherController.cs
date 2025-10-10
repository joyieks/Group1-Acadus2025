using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
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
            // Initialize Supabase client
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            var client = AsiBasecodeDBContext.SupabaseClient;

            // Fetch all users from Supabase (users table is present)
            var usersResponse = await client.From<SupabaseUser>().Get();
            var users = usersResponse.Models;

            // Statistics for teacher dashboard
            ViewBag.StudentCount = users.Count;
            ViewBag.InstructorCount = 0; // Not available in SupabaseUser
            ViewBag.CourseCount = 0; // No courses table in Supabase

            return View();
        }
    }
}


