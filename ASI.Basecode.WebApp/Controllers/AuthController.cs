using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using ASI.Basecode.WebApp.Models;

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
            if (User.Identity.IsAuthenticated)
            {
                // Redirect based on role
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                switch (role)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "Teacher":
                        return RedirectToAction("Courses", "Teacher");
                    case "Student":
                        return RedirectToAction("Index", "Student");
                    default:
                        return RedirectToAction("Index", "Student");
                }
            }
            return View(new LoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please fill in all required fields.");
                return View(model);
            }
            
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var password = model.Password;

            try
            {
                // Authenticate with Supabase via service
                var session = await _supabaseAuthService.SignInAsync(normalizedEmail, password);
                
                if (session?.User != null)
                {
                    // Check if user is confirmed
                    if (session.User.EmailConfirmedAt.HasValue)
                    {
                        // Determine user role and name by checking database tables and admin status
                        var (userRole, userName) = await _supabaseAuthService.GetUserRoleAndNameAsync(session.User.Id);
                        

                        Console.WriteLine($"User {session.User.Email} logged in with role: {userRole}, name: {userName}");



                        // Create claims for the user
                        var claims = new List<Claim>
                        {

                            new Claim(ClaimTypes.NameIdentifier, session.User.Id),
                            new Claim(ClaimTypes.Email, session.User.Email),
                            new Claim(ClaimTypes.Name, $"{session.User.UserMetadata.GetValueOrDefault("first_name", "")} {session.User.UserMetadata.GetValueOrDefault("last_name", "")}"),
                            new Claim(ClaimTypes.Role, userRole),  // ? CRITICAL: Set the role claim
                            new Claim("SupabaseUserId", session.User.Id)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,  // Remember me functionality
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                            AllowRefresh = true
                        };

                        // Sign in the user with cookie authentication
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties
                        );

                        // Persist essentials in server-side session for later flows
                        try
                        {
                            HttpContext.Session.SetString("UserEmail", session.User.Email ?? string.Empty);
                            HttpContext.Session.SetString("SupabaseUserId", session.User.Id ?? string.Empty);
                            HttpContext.Session.SetString("UserRole", userRole);
                        }
                        catch { }
                        
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
                        ModelState.AddModelError(string.Empty, "⚠️ Email not verified. Please check your inbox and verify your email address before logging in.");
                        return View(model);
                    }
                }
                else
                {
                    // Session or user is null - invalid credentials
                    ModelState.AddModelError(string.Empty, "❌ Invalid email or password. Please check your credentials and try again.");
                    return View(model);
                }
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException gex)
            {
                // Supabase-specific authentication errors
                Console.WriteLine($"Supabase Auth Error: {gex.Message}");
                
                if (gex.Message.Contains("Invalid login credentials") || gex.Message.Contains("invalid_grant"))
                {
                    ModelState.AddModelError(string.Empty, "❌ Invalid email or password. Please check your credentials and try again.");
                }
                else if (gex.Message.Contains("Email not confirmed"))
                {
                    ModelState.AddModelError(string.Empty, "⚠️ Email not verified. Please check your inbox and verify your email address.");
                }
                else if (gex.Message.Contains("too many requests") || gex.Message.Contains("rate limit"))
                {
                    ModelState.AddModelError(string.Empty, "⏳ Too many login attempts. Please wait a few minutes and try again.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"❌ Login failed: {gex.Message}");
                }
                
                return View(model);
            }
            catch (System.Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Unexpected Auth Error: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Generic error message for unexpected errors
                ModelState.AddModelError(string.Empty, "❌ An unexpected error occurred. Please try again later or contact support if the problem persists.");
                return View(model);
            }
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

        /// <summary>
        /// Shows the forgot password page
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return RedirectToAction("ForgotPassword", "Account");
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Sign out from cookie authentication
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Clear session
                HttpContext.Session.Clear();
        
                Console.WriteLine("User logged out successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
            }
            
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Access denied page
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewBag.Message = "You do not have permission to access this resource.";

            return View();
        }
    }
}



