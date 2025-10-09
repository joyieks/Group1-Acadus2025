using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AdminController : Controller
    {
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
            return View();
        }
    }
}


