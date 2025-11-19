using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;

        public HomeController(ISupabaseAuthService supabaseAuthService)
        {
            _supabaseAuthService = supabaseAuthService;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Check user role from claims and redirect to appropriate dashboard
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Index", "Teacher");
                }
                else if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    // Default to login if role not recognized
                    return RedirectToAction("Login", "Auth");
                }
            }
            else
            {
                // Not authenticated, redirect to login page
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Redirect to StudentController.Profile() for students, or show profile for other users
        public async Task<IActionResult> Profile()
        {
            // Check if user is a student - if so, redirect to StudentController
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Profile", "Student");
            }

            // For other users (or if role check fails), try to load basic profile
            try
            {
                var supabaseUserId = HttpContext.Session.GetString("SupabaseUserId") ?? 
                                    User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? 
                                    User.FindFirstValue("sub");
                
                if (string.IsNullOrWhiteSpace(supabaseUserId))
                {
                    ViewBag.NoDataMessage = "Session expired. Please log in again.";
                    return View("~/Views/Shared/Profile.cshtml", new StudentProfileViewModel());
                }

                var model = new StudentProfileViewModel();
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Load user info
                var user = await client.From<ASI.Basecode.Data.Models.SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Get();

                var userData = user?.Models?.FirstOrDefault();
                if (userData != null)
                {
                    model.FirstName = userData.FirstName;
                    model.MiddleName = userData.MiddleName;
                    model.LastName = userData.LastName;
                    model.FullName = string.Join(" ", new[] { userData.FirstName, userData.MiddleName, userData.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    model.EmailAddress = userData.Email;
                    model.PhoneNumber = userData.ContactNumber;
                    model.StudentId = userData.UserDisplayId ?? "N/A";
                    model.Status = userData.IsActive ?? false ? "Active" : "Inactive";
                }

                // Profile image
                model.ProfileImageUrl = await _supabaseAuthService.GetUserProfileImageUrlAsync(supabaseUserId);
                if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    model.ProfileImageUrl = uploadedUrl;
                }

                // Password last updated (default to now if not available)
                model.PasswordLastUpdated = DateTime.Now.AddMonths(-1);

                return View("~/Views/Shared/Profile.cshtml", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profile: {ex.Message}");
                ViewBag.NoDataMessage = "Error loading profile data. Please try again.";
                return View("~/Views/Shared/Profile.cshtml", new StudentProfileViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> UploadProfilePhoto(IFormFile profilePhoto, string? returnUrl)
        {
            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                TempData["UploadError"] = "Please select an image to upload.";
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Profile", "Home")!);
            }

            var extension = Path.GetExtension(profilePhoto.FileName);
            var permittedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!permittedExtensions.Contains(extension))
            {
                TempData["UploadError"] = "Unsupported file type. Please upload a JPG, PNG, GIF, or WEBP image.";
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Profile", "Home")!);
            }

            const long maxSizeBytes = 500 * 1024; // 500 KB
            if (profilePhoto.Length > maxSizeBytes)
            {
                TempData["UploadError"] = "Image too large. Maximum size is 500 KB.";
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Profile", "Home")!);
            }

            var supabaseUserId = HttpContext.Session.GetString("SupabaseUserId");
            if (string.IsNullOrWhiteSpace(supabaseUserId))
            {
                TempData["UploadError"] = "Your session has expired. Please log in again.";
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Profile", "Home")!);
            }

            try
            {
                using (var stream = profilePhoto.OpenReadStream())
                {
                    var objectPath = await _supabaseAuthService.UploadProfileImageAsync(
                        supabaseUserId,
                        profilePhoto.FileName,
                        stream,
                        profilePhoto.ContentType ?? "image/jpeg");

                    // Generate a display URL (signed for private buckets)
                    var displayUrl = await _supabaseAuthService.GetProfileImageUrlAsync(objectPath, 3600);

                    await _supabaseAuthService.SetUserProfileImageUrlAsync(supabaseUserId, displayUrl, objectPath);
                    TempData["UploadedProfileUrl"] = displayUrl;
                }
            }
            catch (Exception ex)
            {
                TempData["UploadError"] = $"Failed to upload image: {ex.Message}";
            }

            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Profile", "Home")!);
        }
    }
}






