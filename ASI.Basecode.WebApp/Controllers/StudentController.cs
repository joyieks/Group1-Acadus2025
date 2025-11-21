using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Student")]

    public class StudentController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IStudentCourseService _studentCourseService;
        private readonly IStudentService _studentService;
        private readonly ITeacherCourseActivityRepository _activityRepository;

        public StudentController(ISupabaseAuthService supabaseAuthService, IStudentCourseService studentCourseService, IStudentService studentService, ITeacherCourseActivityRepository activityRepository)
        {
            _supabaseAuthService = supabaseAuthService;
            _studentCourseService = studentCourseService;
            _studentService = studentService;
            _activityRepository = activityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get the Supabase user ID from claims
            var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(supabaseUserId))
                return Unauthorized();

            // Get student ID from the user record using Supabase ID
            var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
            if (string.IsNullOrWhiteSpace(studentId))
                return Unauthorized();

            var dashboard = await _studentCourseService.GetStudentDashboardAsync(studentId);
            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            // Get the Supabase user ID from claims
            var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(supabaseUserId))
                return Unauthorized();

            // Get student ID from the user record using Supabase ID
            var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
            if (string.IsNullOrWhiteSpace(studentId))
                return Unauthorized();

            // Fetch courses
            List<CourseModel> enrolledCourses = await _studentCourseService.GetCoursesByStudentAsync(studentId);

            if (enrolledCourses == null || !enrolledCourses.Any())
            {
                ViewData["Message"] = "No enrolled courses found.";
                return View(Array.Empty<CourseCardViewModel>());
            }

            // Map CourseModel → CourseCardViewModel
            var courseViewModels = enrolledCourses.Select(c => new CourseCardViewModel
            {
                Id = c.Id,
                CourseCode = c.Code ?? "N/A",
                CourseTitle = c.Name ?? "Untitled Course",
                SemesterInfo = c.SemesterId.ToString(),
                CardColor = GetRandomCardColor()
            }).ToArray();

            return View(courseViewModels);
        }



        private string GetRandomCardColor()
        {
            // simple random pastel green variants
            var colors = new[] { "#E8F9E8", "#D1FAE5", "#A7F3D0", "#6EE7B7" };
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }

        public async Task<IActionResult> CourseDetails(string courseId, string tab = "grades", int page = 1)
        {
            // Get the Supabase user ID from claims
            var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(supabaseUserId))
                return Unauthorized();

            // Get student ID from the user record using Supabase ID
            var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
            if (string.IsNullOrWhiteSpace(studentId))
                return Unauthorized();

            var data = await _studentCourseService.GetCourseDetailsAsync(studentId, courseId);
            Debug.WriteLine("StudentId from Identity = " + studentId);
            var json = System.Text.Json.JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.Diagnostics.Debug.WriteLine("=== COURSE DETAILS DATA ===");
            System.Diagnostics.Debug.WriteLine(json);


            const int pageSize = 10;

            var list = tab switch
            {
                "feedback" => data.Feedbacks.Cast<object>().ToList(),
                _ => data.Activities.Cast<object>().ToList(),
            };

            var totalItems = list.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var paginated = list.Skip(skip).Take(pageSize).ToList();

            data.CurrentPage = page;
            data.TotalPages = totalPages;

            if (tab == "feedback")
                data.Feedbacks = paginated.Cast<StudentCourseDetailsViewModel.FeedbackItem>().ToList();
            else
                data.Activities = paginated.Cast<StudentCourseDetailsViewModel.ActivityItem>().ToList();
            {

                return View(data);

            }
        }


        // -------------------- Reports Controller --------------------
        public async Task<IActionResult> Reports()
        {
            // Get the Supabase user ID from claims
            var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(supabaseUserId))
                return Unauthorized();

            // Get student ID from the user record using Supabase ID
            var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
            if (string.IsNullOrWhiteSpace(studentId))
                return Unauthorized();

            var reports = await _studentCourseService.GetStudentReportsAsync(studentId);

            var viewModel = new StudentReportViewModel
            {
                Reports = reports
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadGradeReport()
        {
            var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(supabaseUserId))
                return Unauthorized();

            var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
            if (string.IsNullOrWhiteSpace(studentId))
                return Unauthorized();

            var reports = await _studentCourseService.GetStudentReportsAsync(studentId);

            var pdfBytes = _studentCourseService.GenerateStudentReportPdf(reports); // You implement this
            return File(pdfBytes, "application/pdf", "GradeReport.pdf");
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var supabaseUserId = HttpContext.Session.GetString("SupabaseUserId") ?? 
                                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                                User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(supabaseUserId))
            {
                ViewBag.NoDataMessage = "Session expired. Please log in again.";
                return View("~/Views/Shared/Profile.cshtml", new StudentProfileViewModel());
            }

            var model = new StudentProfileViewModel();
            try
            {
                Console.WriteLine($"=== Loading Student Profile ===");
                Console.WriteLine($"SupabaseUserId: {supabaseUserId}");

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Load user info (all personal data is in users table)
                var userQuery = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Get();
                var user = userQuery?.Models?.FirstOrDefault();

                if (user == null)
                {
                    Console.WriteLine($"User not found for SupabaseUserId: {supabaseUserId}");
                    ViewBag.NoDataMessage = "User profile not found. Please contact your administrator.";
                    return View("~/Views/Shared/Profile.cshtml", model);
                }

                Console.WriteLine($"User found: {user.FirstName} {user.LastName}");

                model.FirstName = user.FirstName;
                model.MiddleName = user.MiddleName;
                model.LastName = user.LastName;
                model.Suffix = user.Suffix;
                model.PhoneNumber = user.ContactNumber;
                model.StudentId = user.UserDisplayId;
                model.FullName = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName, user.Suffix }.Where(s => !string.IsNullOrWhiteSpace(s)));
                model.EmailAddress = user.Email;
                model.Status = user.IsActive ?? false ? "Active" : "Inactive";
                
                // Set password last updated date (use current date as default since we don't track this yet)
                model.PasswordLastUpdated = DateTime.Now;

                // Get studentProfile for academic info
                try
                {
                    var studentProfileQuery = await client.From<Student>()
                        .Where(x => x.StudentId == supabaseUserId)
                        .Get();
                    var studentProfile = studentProfileQuery?.Models?.FirstOrDefault();

                    if (studentProfile != null)
                    {
                        Console.WriteLine($"StudentProfile found: ID={studentProfile.Id}");
                        model.Department = studentProfile.DepartmentId?.ToString() ?? "N/A";
                        model.Course = studentProfile.ProgramId?.ToString() ?? "N/A";
                        model.YearLevel = studentProfile.YearLevel?.ToString() ?? "N/A";

                        // Address (primary) - wrapped in try-catch to handle missing address gracefully
                        try
                        {
                            var studentAddressQuery = await client.From<StudentAddress>()
                                .Where(sa => sa.StudentId == studentProfile.Id && sa.IsPrimary == true)
                                .Get();
                            var studentAddress = studentAddressQuery?.Models?.FirstOrDefault();

                            if (studentAddress != null)
                            {
                                var addressQuery = await client.From<Address>()
                                    .Where(a => a.Id == studentAddress.AddressId)
                                    .Get();
                                var address = addressQuery?.Models?.FirstOrDefault();
                                
                                if (address != null)
                                {
                                    model.HouseNumber = address.HouseNumber;
                                    model.Street = address.StreetName;
                                    model.Subdivision = address.Subdivision;
                                    model.Barangay = address.Barangay;
                                    model.City = address.City;
                                    model.Province = address.Province;
                                    model.ZipCode = address.ZipCode;
                                }
                            }
                        }
                        catch (Exception addrEx)
                        {
                            Console.WriteLine($"Warning: Could not load address: {addrEx.Message}");
                            // Continue without address - not critical
                        }

                        // Emergency contact (primary) - wrapped in try-catch to handle missing contact gracefully
                        try
                        {
                            var emergencyQuery = await client.From<StudentEmergencyContact>()
                                .Where(ec => ec.StudentId == studentProfile.Id && ec.IsPrimary == true)
                                .Get();
                            var emergency = emergencyQuery?.Models?.FirstOrDefault();
                            
                            if (emergency != null)
                            {
                                var contactQuery = await client.From<Contact>()
                                    .Where(c => c.Id == emergency.ContactId)
                                    .Get();
                                var contact = contactQuery?.Models?.FirstOrDefault();
                                
                                if (contact != null)
                                {
                                    model.EmergencyFirstName = contact.FirstName;
                                    model.EmergencyMiddleName = contact.MiddleName;
                                    model.EmergencyLastName = contact.LastName;
                                    model.EmergencySuffix = contact.Suffix;
                                    model.EmergencyContactNumber = contact.ContactNumber;
                                    model.EmergencyRelationship = emergency.Relationship;
                                }
                            }
                        }
                        catch (Exception contactEx)
                        {
                            Console.WriteLine($"Warning: Could not load emergency contact: {contactEx.Message}");
                            // Continue without emergency contact - not critical
                        }
                    }
                    else
                    {
                        Console.WriteLine($"StudentProfile not found for SupabaseUserId: {supabaseUserId}");
                        // Set default values for academic info
                        model.Department = "N/A";
                        model.Course = "N/A";
                        model.YearLevel = "N/A";
                    }
                }
                catch (Exception studentProfileEx)
                {
                    Console.WriteLine($"Warning: Error loading student profile details: {studentProfileEx.Message}");
                    // Continue with basic user info - academic info is optional
                    model.Department = "N/A";
                    model.Course = "N/A";
                    model.YearLevel = "N/A";
                }

                // Profile image from Auth metadata (set by upload)
                try
                {
                    model.ProfileImageUrl = await _supabaseAuthService.GetUserProfileImageUrlAsync(supabaseUserId);
                }
                catch (Exception imgEx)
                {
                    Console.WriteLine($"Warning: Could not load profile image: {imgEx.Message}");
                    // Continue without profile image - not critical
                }

                // If recent upload exists in this session, prefer it
                if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    model.ProfileImageUrl = uploadedUrl;
                }

                Console.WriteLine($"Profile loaded successfully. HasData: {model.HasData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading student profile: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ViewBag.NoDataMessage = "Error loading profile data. Please try again.";
                return View("~/Views/Shared/Profile.cshtml", new StudentProfileViewModel());
            }

            // Only show "No profile data available" if we have absolutely no data
            if (!model.HasData && string.IsNullOrWhiteSpace(model.FirstName) && string.IsNullOrWhiteSpace(model.LastName))
            {
                ViewBag.NoDataMessage = "No profile data available.";
            }

            return View("~/Views/Shared/Profile.cshtml", model);
        }

        /// <summary>
        /// Gets the student database ID from the Supabase user ID
        /// </summary>
        private async Task<string> GetStudentIdFromSupabaseIdAsync(string supabaseUserId)
        {
            try
            {
                var student = await _studentService.GetStudentBySupabaseIdAsync(supabaseUserId);
                return student?.UserTypeId.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student ID from Supabase ID: {ex.Message}");
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitActivity(int activityId, int courseId, string submissionContent)
        {
            try
            {
                // Get the student ID
                var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(supabaseUserId))
                    return Unauthorized();

                var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
                if (string.IsNullOrWhiteSpace(studentId))
                    return Unauthorized();

                // Validate input
                if (string.IsNullOrWhiteSpace(submissionContent))
                {
                    TempData["ErrorMessage"] = "Please provide your answer or work.";
                    return RedirectToAction("CourseDetails", new { courseId = courseId });
                }

                // Get activity details to check due date
                var activity = await _activityRepository.GetActivityByIdAsync(activityId);
                if (activity == null)
                {
                    TempData["ErrorMessage"] = "Activity not found.";
                    return RedirectToAction("CourseDetails", new { courseId = courseId });
                }

                // Check if submission already exists
                var existing = await _activityRepository.GetSubmissionAsync(activityId, studentId);

                // Determine submission status based on due date
                var currentTime = DateTime.UtcNow;
                
                // Handle due date - ensure proper timezone handling
                DateTime dueDateUtc;
                if (activity.DueDate.Kind == DateTimeKind.Unspecified)
                {
                    // If timezone is unspecified, assume it's stored as UTC in database
                    dueDateUtc = DateTime.SpecifyKind(activity.DueDate, DateTimeKind.Utc);
                }
                else if (activity.DueDate.Kind == DateTimeKind.Local)
                {
                    // Convert local time to UTC
                    dueDateUtc = activity.DueDate.ToUniversalTime();
                }
                else
                {
                    // Already UTC
                    dueDateUtc = activity.DueDate;
                }
                
                // Compare times - if current time is greater than due date, it's late
                var isLate = currentTime > dueDateUtc;
                var submissionStatus = isLate ? "Late" : "Submitted";
                
                // Force status to be set (never null)
                if (string.IsNullOrWhiteSpace(submissionStatus))
                {
                    submissionStatus = "Submitted"; // Default fallback
                }

                Console.WriteLine($"=== SubmitActivity ===");
                Console.WriteLine($"ActivityId: {activityId}, StudentId: {studentId}");
                Console.WriteLine($"Original Due Date from DB: {activity.DueDate} (Kind: {activity.DueDate.Kind})");
                Console.WriteLine($"Due Date (UTC): {dueDateUtc} (UTC)");
                Console.WriteLine($"Current Time (UTC): {currentTime} (UTC)");
                Console.WriteLine($"Time Difference: {(currentTime - dueDateUtc).TotalHours:F2} hours");
                Console.WriteLine($"Is Late: {isLate} (Current > Due Date: {currentTime > dueDateUtc})");
                Console.WriteLine($"Submission Status: '{submissionStatus}'");
                Console.WriteLine($"SubmissionContent length: {submissionContent?.Length ?? 0}");
                Console.WriteLine($"SubmissionContent preview: {submissionContent?.Substring(0, Math.Min(50, submissionContent?.Length ?? 0))}...");

                var submission = new ActivitySubmissionModel
                {
                    ActivityId = activityId,
                    StudentId = studentId,
                    SubmissionContent = submissionContent,
                    SubmissionStatus = submissionStatus,
                    CreatedAt = currentTime,
                    Score = existing?.Score ?? 0  // Keep existing score if resubmitting
                };

                await _activityRepository.SaveSubmissionAsync(submission);
                
                Console.WriteLine($"Submission saved successfully with status: {submissionStatus}");

                if (isLate)
                {
                    TempData["SuccessMessage"] = "Activity submitted successfully! (Note: This submission is late)";
                }
                else
                {
                    TempData["SuccessMessage"] = "Activity submitted successfully!";
                }
                return RedirectToAction("CourseDetails", new { courseId = courseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting activity: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while submitting your activity. Please try again.";
                return RedirectToAction("CourseDetails", new { courseId = courseId });
            }
        }

    }
}