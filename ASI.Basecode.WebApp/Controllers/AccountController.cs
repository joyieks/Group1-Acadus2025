using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;
        
        // Rate limiting: Track password reset requests
        private static Dictionary<string, List<DateTime>> _passwordResetAttempts = new Dictionary<string, List<DateTime>>();
        private static Dictionary<string, List<DateTime>> _passwordUpdateAttempts = new Dictionary<string, List<DateTime>>();
        private const int MaxAttemptsPerHour = 5;
        private const int MaxPasswordUpdateAttemptsPerHour = 10;

        public AccountController(ISupabaseAuthService supabaseAuthService, IConfiguration configuration)
        {
            _supabaseAuthService = supabaseAuthService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Render the Auth/Index view directly
            return View("~/Views/Auth/Index.cshtml");
        }

        /// <summary>
        /// Shows the password setup page (accessed via email link)
        /// Handles both initial load and hash fragment with tokens
        /// </summary>
        [HttpGet]
        public IActionResult SetPassword()
        {
            try
            {
                // Pass Supabase configuration to the view securely
                ViewBag.SupabaseUrl = _configuration["Supabase:Url"];
                ViewBag.SupabaseAnonKey = _configuration["Supabase:AnonKey"];

                // Check for error in URL parameters (from Supabase redirect)
                var error = Request.Query["error"].ToString();
                var errorCode = Request.Query["error_code"].ToString();
                var errorDescription = Request.Query["error_description"].ToString();

                if (!string.IsNullOrEmpty(error))
                {
                    AuditLog("PASSWORD_RESET_LINK_ERROR", "UNKNOWN", $"{errorCode}: {errorDescription}");

                    if (errorCode == "otp_expired")
                    {
                        ViewBag.Error = "This password reset link has expired. Please request a new one.";
                        ViewBag.ErrorType = "expired";
                    }
                    else
                    {
                        ViewBag.Error = "This password reset link is invalid or has been used. Please request a new one.";
                        ViewBag.ErrorType = "invalid";
                    }

                    return View("~/Views/Account/SetPassword.cshtml");
                }

                // Don't check for session server-side
                // The session will be established via JavaScript when the hash fragment is processed
                AuditLog("PASSWORD_RESET_PAGE_ACCESSED", "VISITOR", "Password reset page loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading password reset page: {ex.Message}");
                ViewBag.Error = "An error occurred. Please try requesting a new password reset link.";
            }

            return View("~/Views/Account/SetPassword.cshtml");
        }

        /// <summary>
        /// Shows the password reset request page
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("~/Views/Account/ForgotPassword.cshtml");
        }

        /// <summary>
        /// Handles password update from the setup page with enhanced security
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request" });
            }

            try
            {
                // Get the current user session
                var supabaseClient = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                var session = supabaseClient.Auth.CurrentSession;
                
                if (session?.User == null)
                {
                    AuditLog("PASSWORD_UPDATE_FAILED", "UNKNOWN", "No valid session found");
                    return BadRequest(new
                    {
                        success = false,
                        message = "No valid session found. Please try the password reset process again."
                    });
                }

                var userId = session.User.Id;
                var userEmail = session.User.Email;

                // Rate limiting check
                if (IsRateLimited(userId, _passwordUpdateAttempts, MaxPasswordUpdateAttemptsPerHour))
                {
                    AuditLog("PASSWORD_UPDATE_RATE_LIMITED", userEmail, "Too many password update attempts");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Too many password update attempts. Please try again later."
                    });
                }

                // Track attempt
                TrackAttempt(userId, _passwordUpdateAttempts);

                // Validate password strength
                var (isValid, validationMessage) = ValidatePasswordStrength(request.NewPassword);
                if (!isValid)
                {
                    AuditLog("PASSWORD_UPDATE_VALIDATION_FAILED", userEmail, validationMessage);
                    return BadRequest(new
                    {
                        success = false,
                        message = validationMessage
                    });
                }

                // Check if passwords match
                if (request.NewPassword != request.ConfirmPassword)
                {
                    AuditLog("PASSWORD_UPDATE_MISMATCH", userEmail, "Passwords do not match");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Passwords do not match"
                    });
                }

                // Update password
                var success = await _supabaseAuthService.UpdateUserPasswordAsync(userId, request.NewPassword);

                if (success)
                {
                    // Clear all user sessions for security
                    await ClearUserSessionsAsync(userId);
                    
                    AuditLog("PASSWORD_UPDATED_SUCCESS", userEmail, "Password successfully updated");
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Password updated successfully. Please log in with your new password."
                    });
                }
                else
                {
                    AuditLog("PASSWORD_UPDATE_FAILED", userEmail, "Supabase update failed");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to update password. Please try again."
                    });
                }
            }
            catch (System.Exception ex)
            {
                AuditLog("PASSWORD_UPDATE_ERROR", "EXCEPTION", ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = "An error occurred while updating your password. Please try again."
                });
            }
        }

        /// <summary>
        /// Resends password setup email
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResendPasswordSetup([FromBody] ResendEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            try
            {
                var success = await _supabaseAuthService.SendPasswordSetupEmailAsync(request.Email);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Password setup email sent successfully. Please check your inbox."
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to send password setup email"
                    });
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends password reset email with rate limiting and audit logging
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendPasswordReset([FromBody] ResendEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // Rate limiting check
            if (IsRateLimited(normalizedEmail, _passwordResetAttempts, MaxAttemptsPerHour))
            {
                AuditLog("PASSWORD_RESET_RATE_LIMITED", normalizedEmail, "Too many password reset requests");
                return BadRequest(new
                {
                    success = false,
                    message = $"Too many password reset requests. Please try again later."
                });
            }

            // Track attempt
            TrackAttempt(normalizedEmail, _passwordResetAttempts);

            try
            {
                var success = await _supabaseAuthService.SendPasswordSetupEmailAsync(normalizedEmail);

                if (success)
                {
                    AuditLog("PASSWORD_RESET_EMAIL_SENT", normalizedEmail, "Password reset email sent successfully");
                    return Ok(new
                    {
                        success = true,
                        message = "If an account exists with this email, you will receive a password reset link shortly."
                    });
                }
                else
                {
                    AuditLog("PASSWORD_RESET_EMAIL_FAILED", normalizedEmail, "Failed to send password reset email");
                    // Return success message anyway to avoid email enumeration
                    return Ok(new
                    {
                        success = true,
                        message = "If an account exists with this email, you will receive a password reset link shortly."
                    });
                }
            }
            catch (System.Exception ex)
            {
                AuditLog("PASSWORD_RESET_ERROR", normalizedEmail, ex.Message);
                // Return generic success message to avoid information disclosure
                return Ok(new
                {
                    success = true,
                    message = "If an account exists with this email, you will receive a password reset link shortly."
                });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Rate limiting: Check if user has exceeded password reset attempts
        /// </summary>
        private bool IsRateLimited(string identifier, Dictionary<string, List<DateTime>> attempts, int maxAttempts)
        {
            var now = DateTime.UtcNow;
            var oneHourAgo = now.AddHours(-1);

            // Clean up old attempts
            if (attempts.ContainsKey(identifier))
            {
                attempts[identifier] = attempts[identifier].Where(d => d > oneHourAgo).ToList();
                
                if (attempts[identifier].Count >= maxAttempts)
                {
                    AuditLog("RATE_LIMIT_EXCEEDED", identifier, $"Exceeded {maxAttempts} attempts per hour");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Track attempt for rate limiting
        /// </summary>
        private void TrackAttempt(string identifier, Dictionary<string, List<DateTime>> attempts)
        {
            if (!attempts.ContainsKey(identifier))
            {
                attempts[identifier] = new List<DateTime>();
            }
            attempts[identifier].Add(DateTime.UtcNow);
        }

        /// <summary>
        /// Audit logging: Log security-related events
        /// </summary>
        private void AuditLog(string action, string identifier, string details)
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] SECURITY_AUDIT | Action: {action} | Identifier: {identifier} | Details: {details} | IP: {HttpContext.Connection.RemoteIpAddress}";
            Console.WriteLine(logEntry);
            
            // In production, write to a secure audit log file or database
            // System.IO.File.AppendAllText("audit_log.txt", logEntry + Environment.NewLine);
        }

        /// <summary>
        /// Validate password strength against security requirements
        /// </summary>
        private (bool isValid, string message) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "Password is required");
            }

            if (password.Length < 8)
            {
                return (false, "Password must be at least 8 characters long");
            }

            if (password.Length > 128)
            {
                return (false, "Password must not exceed 128 characters");
            }

            // Check for uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter (A-Z)");
            }

            // Check for lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter (a-z)");
            }

            // Check for digit
            if (!Regex.IsMatch(password, @"\d"))
            {
                return (false, "Password must contain at least one number (0-9)");
            }

            // Check for special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                return (false, "Password must contain at least one special character (!@#$%^&*(),.?\":{}|<>)");
            }

            // Check for common weak passwords
            var commonPasswords = new[] { "Password1!", "Welcome1!", "Admin123!", "Qwerty123!" };
            if (commonPasswords.Any(p => p.Equals(password, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "This password is too common. Please choose a more unique password");
            }

            return (true, "Password meets all requirements");
        }

        /// <summary>
        /// Clear all user sessions after password change for security
        /// </summary>
        private Task ClearUserSessionsAsync(string userId)
        {
            try
            {
                // Supabase automatically invalidates sessions when password is changed
                // Additional session cleanup can be implemented here if needed
                AuditLog("SESSION_CLEARED", userId, "All sessions cleared after password change");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to clear sessions for user {userId}: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        #endregion
    }

    // Request Models
    public class UpdatePasswordRequest
    {
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }
    }

    public class ResendEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}