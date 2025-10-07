using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Render the Auth/Index view directly
            return View("~/Views/Auth/Index.cshtml");
        }
    }
}




