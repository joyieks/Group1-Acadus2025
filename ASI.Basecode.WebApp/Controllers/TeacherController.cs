using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using ASI.Basecode.WebApp.Models;

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
        public IActionResult Index()
        {
            var model = new TeacherDashboardViewModel
            {
                TotalActivities = 0,
                GradedActivities = 0,
                TotalCoursesHandled = 0
            };
            return View(model);
        }

        /// <summary>
        /// Displays the teacher's activities view.
        /// </summary>
        /// <returns>The activities view.</returns>
        [HttpGet]
        public IActionResult Activities()
        {
            return View();
        }

        /// <summary>
        /// Displays the teacher's grades view.
        /// </summary>
        /// <returns>The grades view.</returns>
        [HttpGet]
        public IActionResult Grades()
        {
            return View();
        }
    }
}


