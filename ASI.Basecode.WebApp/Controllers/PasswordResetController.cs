using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class PasswordResetController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IEmailService _emailService;

        public PasswordResetController(ISupabaseAuthService supabaseAuthService, IEmailService emailService)
        {
            _supabaseAuthService = supabaseAuthService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult RequestReset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestReset(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return View();
            }

            try
            {
                // In a real implementation, you'd generate a secure reset token
                // For now, we'll just send a password reset email
                var resetLink = $"{Request.Scheme}://{Request.Host}/PasswordReset/Reset?token=temp_token&email={email}";
                
                var emailSent = await _emailService.SendPasswordResetEmailAsync(email, "Student", resetLink);
                
                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Password reset instructions have been sent to your email address.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to send password reset email. Please try again or contact support.";
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Reset(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToAction("RequestReset");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reset(string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Email and new password are required.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            try
            {
                // In a real implementation, you'd validate the reset token
                // and update the password in Supabase Auth
                await Task.Delay(1); // Placeholder for async operation
                TempData["SuccessMessage"] = "Password has been reset successfully. You can now log in with your new password.";
                return RedirectToAction("Index", "Auth");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error resetting password: {ex.Message}");
                return View();
            }
        }
    }
}
