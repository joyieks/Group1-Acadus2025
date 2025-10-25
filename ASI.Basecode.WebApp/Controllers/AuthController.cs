using Microsoft.AspNetCore.Mvc;
using Acadus___Alliance_Project_2025.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;

        public AuthController(ISupabaseAuthService supabaseAuthService, IConfiguration configuration)
        {
            _supabaseAuthService = supabaseAuthService;
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

            try
            {
                // Authenticate with Supabase
                var supabaseClient = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                var session = await supabaseClient.Auth.SignIn(normalizedEmail, password);
                
                if (session?.User != null)
                {
                    // Check if user is confirmed
                    if (session.User.EmailConfirmedAt.HasValue)
                    {
                        // Determine user role by checking database tables and admin status
                        var userRole = await _supabaseAuthService.GetUserRoleAsync(session.User.Id);
                        
                        Console.WriteLine($"User {session.User.Email} logged in with role: {userRole}");
                        
                        // Redirect based on user role
                        switch (userRole)
                        {
                            case "Admin":
                                return RedirectToAction("Dashboard", "Admin");
                            case "Teacher":
                                return RedirectToAction("Index", "Teacher");
                            case "Student":
                                return RedirectToAction("Index", "Student");
                            default:
                                // Default to Student if role not recognized
                                return RedirectToAction("Index", "Student");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                        return View(model);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Supabase Auth Error: {ex.Message}");
            }

            // If authentication failed, show error message
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
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

        /// <summary>
        /// Handles password setup from Supabase email link
        /// </summary>
        [HttpGet]
        public IActionResult SetPassword()
        {
            // Redirect to AccountController's SetPassword action
            return RedirectToAction("SetPassword", "Account");
        }
    }
}



