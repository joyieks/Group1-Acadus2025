using ASI.Basecode.Webapp.Models;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Mock data example (for testing)
        public IActionResult Profile() //Ana jako for all users daw ni hence dito na lang sya
        {
            var model = new StudentProfileViewModel
            {
                ProfileImageUrl = null,
                FullName = string.Empty,
                StudentId = string.Empty,
                Program = string.Empty,
                YearLevel = string.Empty,
                EmailAddress = string.Empty,
                PhoneNumber = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                DateOfBirth = null,
                Gender = string.Empty,
                PasswordLastUpdated = default
            };

            // If no profile data exists
            if (!model.HasData)
                ViewBag.NoDataMessage = "No profile data available. Please complete your profile information.";

            return View(model);
        }
    }
}





