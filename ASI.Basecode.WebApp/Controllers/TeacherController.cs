using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TeacherController : Controller
    {
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

        [HttpGet]
        public IActionResult Activities()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Grades()
        {
            return View();
        }
    }
}


