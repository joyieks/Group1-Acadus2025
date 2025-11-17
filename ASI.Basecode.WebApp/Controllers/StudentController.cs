﻿using ASI.Basecode.Data.Models;
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
using static ASI.Basecode.Data.Models.StudentDashboardViewModel; 
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Student")]

    public class StudentController : Controller
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IStudentCourseService _studentCourseService;
        private readonly IStudentService _studentService;

        public StudentController(ISupabaseAuthService supabaseAuthService, IStudentCourseService studentCourseService, IStudentService studentService)
        {
            _supabaseAuthService = supabaseAuthService;
            _studentCourseService = studentCourseService;
            _studentService = studentService;
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



        private string GetCourseTitleById(string courseId)
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

                // Load user info (all personal data is in users table)
                var user = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Single();

                if (user != null)
                {
                    model.FirstName = user.FirstName;
                    model.MiddleName = user.MiddleName;
                    model.LastName = user.LastName;
                    model.FullName = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    model.EmailAddress = user.Email;
                    
                    // Get studentProfile for academic info
                    var studentProfile = await client.From<Student>()
                      .Where(x => x.StudentId == supabaseUserId)
                    .Single();
                    
                  model.Department = studentProfile?.DepartmentId?.ToString() ?? "N/A";
                  model.Course = studentProfile?.ProgramId?.ToString() ?? "N/A";  // Fixed: use ProgramId
  model.YearLevel = studentProfile?.YearLevel?.ToString() ?? "N/A";
    model.Status = user.IsActive ?? false ? "Active" : "Inactive";
                }

               // Address (primary)
             if (user != null)
               {
             // Get studentProfile ID first
              var studentProfile = await client.From<Student>()
.Where(x => x.StudentId == supabaseUserId)
            .Single();
     
    var studentAddress = await client.From<StudentAddress>()
            .Where(sa => sa.StudentId == studentProfile.Id && sa.IsPrimary == true)  // Fixed: use studentProfile.Id (int)
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
          if (user != null)
     {
    var studentProfile = await client.From<Student>()
.Where(x => x.StudentId == supabaseUserId)
         .Single();
   
 var emergency = await client.From<StudentEmergencyContact>()
 .Where(ec => ec.StudentId == studentProfile.Id && ec.IsPrimary == true)  // Fixed: use studentProfile.Id (int)
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
    }
}