using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(TeacherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _teacherService.CreateTeacherAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "Teacher registered successfully! Password setup email has been sent.";
                    return RedirectToAction("AddTeacher");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to register teacher. Please try again.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // This would typically fetch and display a list of teachers
            // For now, just return the view
            return View();
        }
    }
}