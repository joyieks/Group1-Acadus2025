using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
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
                // Check user role and redirect to appropriate dashboard
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
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
                    // Default to Student if role not recognized
                    return RedirectToAction("Index", "Student");
                }
            }
            else
            {
                // Not authenticated, redirect to landing page
                return RedirectToAction("Index", "Auth");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Mock data example (for testing)
        public IActionResult Profile()
        {
            var model = new ASI.Basecode.WebApp.Models.StudentProfileViewModel
            {
                // ===== Basic Profile Information =====
                ProfileImageUrl = "/images/sample-profile.jpg",
                FullName = "Juan Dela Cruz",


                // ===== Personal Info =====
                StudentId = "123",
                Status = "Active",
                FirstName = "Juan",
                MiddleName = "Santos",
                LastName = "Dela Cruz",
                Suffix = "",
                DateOfBirth = "May 3, 2002",
                Gender = "Male",
                Course = "Bachelor of Science in Information Technology",
                YearLevel = "4",
                Department = "College of Computer Studies",
                EmailAddress = "juan.delacruz@example.com",
                PhoneNumber = "09171234567",

                // ===== Address Information =====
                HouseNumber = "123",
                Street = "Maple Street",
                Subdivision = "Sunnyvale Subdivision",
                Barangay = "Barangay Mabini",
                City = "Quezon City",
                Province = "Metro Manila",
                ZipCode = "1100",

                // ===== Emergency Contact Information =====
                EmergencyFirstName = "Maria",
                EmergencyMiddleName = "Reyes",
                EmergencyLastName = "Dela Cruz",
                EmergencySuffix = "",
                EmergencyContactNumber = "09181234567",
                EmergencyRelationship = "Mother",

                // ===== Security Info =====
                PasswordLastUpdated = DateTime.Now.AddMonths(-2)
            };

            // Prefer recently uploaded image if present
            if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
            {
                model.ProfileImageUrl = uploadedUrl;
            }

            // If no profile data exists
            if (!model.HasData)
                ViewBag.NoDataMessage = "No profile data available. Please complete your profile information.";

            return View(model);
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






