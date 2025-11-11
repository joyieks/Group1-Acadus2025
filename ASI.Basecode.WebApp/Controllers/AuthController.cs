using Microsoft.AspNetCore.Mvc;
using Acadus___Alliance_Project_2025.Models;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data;
using static ASI.Basecode.Resources.Constants.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Session;
using DataModels = ASI.Basecode.Data.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var password = model.Password;
            
            // Step 0: Initialize Supabase connection
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            }
            catch (Exception ex)
            {
                // Log Supabase initialization error but continue to dev fallback
                System.Diagnostics.Debug.WriteLine($"Supabase initialization failed: {ex.Message}");
            }
            
            // Step 1: Try authenticating via UserService (Supabase/Database)
            try
            {
                var user = new DataModels.User();
                var authResult = _userService.AuthenticateUser(normalizedEmail, password, ref user);
                
                if (authResult == LoginResult.Success && user != null)
                {
                    // Fetch user roles from database
                    var client = AsiBasecodeDBContext.SupabaseClient;
                    var userRoles = await client.From<DataModels.UserRole>()
                        .Where(ur => ur.userId == user.id)
                        .Get();
                    
                    // Store user in session
                    HttpContext.Session.SetString("UserId", user.id.ToString());
                    HttpContext.Session.SetString("UserEmail", user.email ?? "User");
                    HttpContext.Session.SetString("UserName", $"{user.firstName} {user.lastName}");
                    
                    // Store first role (or could store all roles as comma-separated)
                    if (userRoles?.Models?.Count > 0)
                    {
                        HttpContext.Session.SetString("UserRoleId", userRoles.Models[0].roleId.ToString());
                    }
                    
                    // Redirect based on role (this would need to be enhanced with actual role data)
                    return RedirectToAction("Index", "Student");
                }
            }
            catch (Exception ex)
            {
                // If database access fails, continue to dev user fallback
                // This allows development to work even if database isn't configured
                System.Diagnostics.Debug.WriteLine($"Database authentication failed: {ex.Message}");
            }

            // Step 2: Fall back to hardcoded dev users for testing/development
            // This allows team members to test without Supabase setup
            var devUser = AuthenticateDevUser(normalizedEmail, password);
            if (devUser != null)
            {
                // Store dev user in session
                HttpContext.Session.SetString("UserId", devUser.Email);
                HttpContext.Session.SetString("UserEmail", devUser.Email);
                HttpContext.Session.SetString("UserName", devUser.Name);
                HttpContext.Session.SetString("UserRole", devUser.Role);
                
                // Redirect based on dev user role
                return devUser.Role.ToLowerInvariant() switch
                {
                    "student" => RedirectToAction("Index", "Student"),
                    "teacher" => RedirectToAction("Index", "Teacher"),
                    "admin" => RedirectToAction("Dashboard", "Admin"),
                    _ => RedirectToAction("Index", "Student")
                };
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        /// <summary>
        /// Development/Testing user authentication. 
        /// Allows easy access without Supabase during development.
        /// Remove or comment out in production.
        /// </summary>
        private DevUser AuthenticateDevUser(string email, string password)
        {
            // Check if dev mode is enabled in config
            var devModeEnabled = _configuration.GetValue<bool>("DevMode:Enabled", true);
            if (!devModeEnabled)
                return null;

            // Hardcoded test users for development
            var devUsers = new[]
            {
                new DevUser 
                { 
                    Email = "student@gmail.com", 
                    Password = "student123", 
                    Name = "John Student",
                    Role = "Student"
                },
                new DevUser 
                { 
                    Email = "teacher@gmail.com", 
                    Password = "teacher123", 
                    Name = "Dr. Maria Santos",
                    Role = "Teacher"
                },
                new DevUser 
                { 
                    Email = "admin@gmail.com", 
                    Password = "admin123", 
                    Name = "Admin User",
                    Role = "Admin"
                }
            };

            // Find matching dev user
            var devUser = System.Linq.Enumerable.FirstOrDefault(devUsers, 
                u => u.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase) && 
                     u.Password == password);

            return devUser;
        }

        public IActionResult EmailVerification()
        {
            return View(new EmailVerificationModel());
        }

        [HttpPost]
        public IActionResult EmailVerification(EmailVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return RedirectToAction("OTPVerification");
        }

        public IActionResult OTPVerification()
        {
            return View(new OTPVerificationModel());
        }

        [HttpPost]
        public IActionResult OTPVerification(OTPVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var code = string.Concat(model.Otp1, model.Otp2, model.Otp3, model.Otp4);
            if (code.Length != 4)
            {
                ModelState.AddModelError(string.Empty, "Please enter the 4-digit code.");
                return View(model);
            }

            return RedirectToAction("NewPassword");
        }

        public IActionResult NewPassword()
        {
            return View(new NewPasswordModel());
        }

        [HttpPost]
        public IActionResult NewPassword(NewPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Redirect to success or login
            return RedirectToAction("Login");
        }
    }
}

