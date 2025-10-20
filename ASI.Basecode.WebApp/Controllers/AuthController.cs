using Microsoft.AspNetCore.Mvc;
using Acadus___Alliance_Project_2025.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ISupabaseAuthService supabaseAuthService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _supabaseAuthService = supabaseAuthService;
            _configuration = configuration;
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        
        // Test page for debugging login
        public IActionResult LoginTest()
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
                _logger.LogWarning("Login: ModelState is invalid");
                return View(model);
            }
            
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var password = model.Password;

            _logger.LogInformation($"=== LOGIN ATTEMPT ===");
            _logger.LogInformation($"Email (normalized): '{normalizedEmail}'");
            _logger.LogInformation($"Password length: {password?.Length ?? 0}");

            try
            {
                // Try Supabase Auth first
                var supabaseClient = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                var session = await supabaseClient.Auth.SignIn(normalizedEmail, password);
                
                if (session?.User != null)
                {
                    _logger.LogInformation($"Supabase auth successful for: {normalizedEmail}");
                    // Check if user is confirmed
                    if (session.User.EmailConfirmedAt.HasValue)
                    {
                        // Determine user role by checking database tables
                        var userRole = await _supabaseAuthService.GetUserRoleAsync(session.User.Id);
                        
                        // Redirect based on user role
                        switch (userRole)
                        {
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
                _logger.LogWarning($"Supabase Auth Error for {normalizedEmail}: {ex.Message}");
                Console.WriteLine($"Supabase Auth Error: {ex.Message}");
            }

            // Fallback to hardcoded users for admin/teacher access (for development only)
            var adminEmail = _configuration["Admin:Email"] ?? "admin@gmail.com";
            var adminPassword = _configuration["Admin:Password"] ?? "admin123";
            var teacherEmail = _configuration["Teacher:Email"] ?? "teacher@gmail.com";
            var teacherPassword = _configuration["Teacher:Password"] ?? "teacher123";
            
            _logger.LogInformation($"=== HARDCODED CREDENTIALS CHECK ===");
            _logger.LogInformation($"Teacher Email from config: '{teacherEmail}'");
            _logger.LogInformation($"Teacher Password from config: '{teacherPassword}'");
            _logger.LogInformation($"Email match: {normalizedEmail == teacherEmail}");
            _logger.LogInformation($"Password match: {password == teacherPassword}");
            
            if (normalizedEmail == adminEmail && password == adminPassword)
            {
                _logger.LogInformation($"? Admin login successful for: {normalizedEmail}");
                return RedirectToAction("Dashboard", "Admin");
            }
            if (normalizedEmail == teacherEmail && password == teacherPassword)
            {
                _logger.LogInformation($"? Teacher login successful for: {normalizedEmail}");
                return RedirectToAction("Index", "Teacher");
            }

            _logger.LogWarning($"? Login failed for: {normalizedEmail}");
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



