using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IStudentCourseService _studentCourseService;

        public StudentController(ISupabaseAuthService supabaseAuthService, IStudentCourseService studentCourseService)
        {
            _supabaseAuthService = supabaseAuthService;
            _studentCourseService = studentCourseService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new StudentDashboardViewModel
            {
                RecentlyGradedTasks = new List<StudentDashboardViewModel.TaskItem>(),
                ToBeGradedTasks = new List<StudentDashboardViewModel.TaskItem>()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Courses(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("Invalid student ID.");

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
                CourseCode = c.Code,
                CourseTitle = c.Name,
                SemesterInfo = c.SemesterInfo,
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
            var studentId = "edb1283a-f04b-4922-8c23-d2efbaab257b";
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

            return View(data);
        }

        private string GetCourseTitleById(string? courseId)
        {
            return courseId switch
            {
                "cs101" => "Introduction to Computer Science",
                "math201" => "Discrete Mathematics",
                "eng102" => "Technical Writing",
                "php41" => "Free Elective - PHP",
                _ => "Course Title"
            };
        }


        // -------------------- Notifications Controller --------------------

        [HttpGet]
        public IActionResult Notifications()
        {
            var model = new NotificationsViewModel
            {


                Notifications = new List<NotificationsViewModel.NotificationItem>
        {
            new NotificationsViewModel.NotificationItem
            {
                Title = "New Message from Admin",
                Message = "Your account has been successfully verified.",
                Date = DateTime.Now.AddMinutes(-15),
                IsRead = false
            },
            new NotificationsViewModel.NotificationItem
            {
                Title = "System Maintenance",
                Message = "Scheduled maintenance on October 30, 2025, from 1:00 AM to 3:00 AM.",
                Date = DateTime.Now.AddHours(-2),
                IsRead = true
            },
            new NotificationsViewModel.NotificationItem
            {
                Title = "Grade Update",
                Message = "Your final grade for IT 331 (Database Systems) has been posted.",
                Date = DateTime.Now.AddDays(-1),
                IsRead = false
            }
        }

            };

            if (!model.HasData)
                ViewBag.NoDataMessage = "No notifications available at the moment.";

            return View("~/Views/Shared/Notifications.cshtml", model);
        }

        [HttpGet]
        public PartialViewResult NotificationDropdown()
        {
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>
        {
            new NotificationsViewModel.NotificationItem
            {
                Title = "New Assignment Posted",
                Message = "A new assignment is available in IT 335.",
                Date = DateTime.Now.AddHours(-3),
                IsRead = false
            },
            new NotificationsViewModel.NotificationItem
            {
                Title = "Reminder",
                Message = "Submit your project proposal before October 28.",
                Date = DateTime.Now.AddDays(-2),
                IsRead = true
            }
        }
            };
            return PartialView("_NotificationDropdown", model);
        }

        [HttpGet]
        public IActionResult NotificationCount()
        {

            var count = 0; 
            return Json(new { count });
        }

        // -------------------- Reports Controller --------------------
        public async Task<IActionResult> Reports(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("Student ID is required.");

            var reports = await _studentCourseService.GetStudentReportsAsync(studentId);

            var viewModel = new StudentReportViewModel
            {
                Reports = reports
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var supabaseUserId = HttpContext.Session.GetString("SupabaseUserId");
            if (string.IsNullOrWhiteSpace(supabaseUserId))
            {
                ViewBag.NoDataMessage = "Session expired. Please log in again.";
                return View(new StudentProfileViewModel());
            }

            var model = new StudentProfileViewModel();
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Student record
                var student = await client.From<Student>()
                    .Where(x => x.SupabaseUserId == supabaseUserId)
                    .Single();

                if (student != null)
                {
                    model.FirstName = student.FirstName;
                    model.MiddleName = student.MiddleName;
                    model.LastName = student.LastName;
                    model.FullName = string.Join(" ", new[] { student.FirstName, student.MiddleName, student.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    model.EmailAddress = student.Email;
                    model.Department = student.Department;
                    model.Course = student.Program;
                    model.YearLevel = student.YearLevel.ToString();
                    model.Status = student.IsActive ? "Active" : "Inactive";
                }

                // Address (primary)
                if (student != null)
                {
                    var studentAddress = await client.From<StudentAddress>()
                        .Where(sa => sa.StudentId == student.Id && sa.IsPrimary == true)
                        .Single();

                    if (studentAddress != null)
                    {
                        var address = await client.From<Address>()
                            .Where(a => a.Id == studentAddress.AddressId)
                            .Single();
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

                // Emergency contact (primary)
                if (student != null)
                {
                    var emergency = await client.From<StudentEmergencyContact>()
                        .Where(ec => ec.StudentId == student.Id && ec.IsPrimary == true)
                        .Single();
                    if (emergency != null)
                    {
                        var contact = await client.From<Contact>()
                            .Where(c => c.Id == emergency.ContactId)
                            .Single();
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

                // Profile image from Auth metadata (set by upload)
                model.ProfileImageUrl = await _supabaseAuthService.GetUserProfileImageUrlAsync(supabaseUserId);

                // If recent upload exists in this session, prefer it
                if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    model.ProfileImageUrl = uploadedUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading student profile: {ex.Message}");
                ViewBag.NoDataMessage = "Unable to load profile data at the moment.";
            }

            if (!model.HasData)
                ViewBag.NoDataMessage = "No profile data available.";

            return View(model);
        }
    }
}
