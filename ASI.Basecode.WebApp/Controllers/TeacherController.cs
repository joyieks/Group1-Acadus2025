using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TeacherController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}


