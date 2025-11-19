using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;  // ? For ISupabaseAuthService and ICourseService
using ASI.Basecode.WebApp.Models;  // ? For TeacherCourseViewModel
using ASI.Basecode.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
 namespace ASI.Basecode.WebApp.Controllers

{
    /// <summary>
    /// Controller for teacher-related actions and dashboard statistics.
    /// </summary>
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly ICourseService _courseService;
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly ITeacherCourseActivityService _activityService;
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;
        private readonly ITeacherCourseActivityRepository _activityRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="supabaseAuthService">Supabase authentication service.</param>
        /// <param name="courseService">Course service for database operations.</param>

        public TeacherController(IConfiguration configuration, ISupabaseAuthService supabaseAuthService, ITeacherCourseService teacherCourseService, ICourseService courseService, ITeacherCourseActivityService activityService, IUserService userService, IAuditLogService auditLogService, ITeacherCourseActivityRepository activityRepository)
        {
            _configuration = configuration;
            _supabaseAuthService = supabaseAuthService;
            _teacherCourseService = teacherCourseService;
            _courseService = courseService;
            _activityService = activityService;
            _userService = userService;
            _auditLogService = auditLogService;
            _activityRepository = activityRepository;
        }

        /// <summary>
        /// Displays the teacher dashboard with statistics.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // Get current teacher's Supabase user ID
                var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(supabaseUserId))
                {
                    return View(new TeacherDashboardViewModel
                    {
                        TotalActivities = 0,
                        GradedActivities = 0,
                        TotalCoursesHandled = 0
                    });
                }

                // Get courses taught by this teacher
                var courses = await _courseService.GetCoursesByInstructorAsync(supabaseUserId);
                var totalCourses = courses.Count;

                // Get all activities for teacher's courses
                int totalActivities = 0;
                int gradedActivities = 0;

                foreach (var course in courses)
                {
                    var activities = await client
                        .From<ActivityModel>()
                        .Filter("courseId", Supabase.Postgrest.Constants.Operator.Equals, course.Id)
                        .Get();

                    totalActivities += activities.Models.Count;

                    // Count graded activities (activities with submissions)
                    var activityIds = activities.Models.Select(a => a.Id).ToList();
                    if (activityIds.Any())
                    {
                        var submissions = await client
                            .From<ActivitySubmissionModel>()
                            .Filter("activityId", Supabase.Postgrest.Constants.Operator.In, activityIds.Cast<object>().ToList())
                            .Get();

                        gradedActivities += submissions.Models
                            .Where(s => s.SubmissionStatus == "Graded")
                            .Select(s => s.ActivityId)
                            .Distinct()
                            .Count();
                    }
                }

                var model = new TeacherDashboardViewModel
                {
                    TotalActivities = totalActivities,
                    GradedActivities = gradedActivities,
                    TotalCoursesHandled = totalCourses
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading teacher dashboard: {ex.Message}");
                return View(new TeacherDashboardViewModel
                {
                    TotalActivities = 0,
                    GradedActivities = 0,
                    TotalCoursesHandled = 0
                });
            }
        }
        //    await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
        //    var client = AsiBasecodeDBContext.SupabaseClient;

        //    // TODO: Replace with actual teacher ID from authentication
        //    int teacherId = 1; // Example teacher ID

        //    // Get total activities for the teacher
        //    var activitiesResponse = await client.From<ActivityModel>()
        //        .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
        //        .Get();
        //    var activities = activitiesResponse.Models;
        //    int totalActivities = activities.Count;

        //    // Get graded activities for the teacher
        //    int gradedActivities = activities.Count(a => a.IsGraded);

        //    // Get total courses handled by the teacher
        //    var coursesResponse = await client.From<CourseModel>()
        //        .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
        //        .Get();
        //    var courses = coursesResponse.Models;
        //    int totalCoursesHandled = courses.Count;

        //    // TODO: Implement calendar events retrieval if needed
        //    var calendarEvents = new List<string>();

        //    var model = new TeacherDashboardViewModel
        //    {
        //        TotalActivities = totalActivities,
        //        GradedActivities = gradedActivities,
        //        TotalCoursesHandled = totalCoursesHandled,
        //        CalendarEvents = calendarEvents
        //    };
        //    return View(model);
        //}

        /// <summary>
        /// Displays the teacher's courses view.
        /// </summary>
        /// <returns>The courses view.</returns>
        [HttpGet]
        public async Task<IActionResult> Courses()
        {
   try
     {
      // Get the current teacher's Supabase user ID from claims
     var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    if (string.IsNullOrWhiteSpace(supabaseUserId))
       {
        Console.WriteLine("ERROR: Teacher Supabase User ID not found in claims");
   return View("Courses/Index", new List<TeacherCourseViewModel>());
    }

        Console.WriteLine($"=== LOADING COURSES FOR TEACHER ===");
            Console.WriteLine($"Teacher Supabase User ID: {supabaseUserId}");

     // Get courses taught by this teacher from database
       var dbCourses = await _courseService.GetCoursesByInstructorAsync(supabaseUserId);
       
        Console.WriteLine($"Found {dbCourses.Count} courses for teacher");

        // Map database courses to view model
                var courses = dbCourses.Select((course, index) => new TeacherCourseViewModel
   {
    Id = (int)course.Id,  // ? Cast from long to int
      CourseCode = course.Code ?? "N/A",
     CourseTitle = course.Name ?? "Untitled Course",
         SemesterInfo = GetSemesterInfo(course.SemesterId),
            CardColor = GetCardColor(index)  // Assign colors based on index
    }).ToList();

 if (courses.Count == 0)
   {
        Console.WriteLine("No courses found for this teacher");
       ViewBag.Message = "You are not assigned to any courses yet. Please contact your administrator.";
     }

     return View("Courses/Index", courses.ToArray());
   }
      catch (Exception ex)
          {
    Console.WriteLine($"ERROR loading teacher courses: {ex.Message}");
         Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    ViewBag.Error = "Unable to load courses. Please try again later.";
                return View("Courses/Index", new List<TeacherCourseViewModel>());
            }
        }

      /// <summary>
        /// Helper method to get semester information
        /// </summary>
 private string GetSemesterInfo(long? semesterId)
      {
 if (!semesterId.HasValue)
   return "No Semester Assigned";

     // TODO: You can enhance this to fetch actual semester details from database
      // For now, return a placeholder
      return $"Semester ID: {semesterId}";
        }

        /// <summary>
        /// Helper method to assign card colors based on index
   /// </summary>
private string GetCardColor(int index)
{
            // Cycle through a set of green shades
  var colors = new[]
         {
    "#E8F9E8",  // Light green
   "#D1FAE5",  // Lighter green
       "#A7F3D0",  // Medium green
                "#6EE7B7",  // Medium-dark green
     "#34D399",  // Dark green
  "#10B981"   // Darkest green
        };

   return colors[index % colors.Length];
 }
        /// <summary>
        /// Displays the full course view.
        /// </summary>
        /// <returns>The full course view.</returns>
        [HttpGet]
        public async Task<IActionResult> FullCourseView(int id)
        {
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            var client = AsiBasecodeDBContext.SupabaseClient;

            // Fetch the course from the database
            var courseResponse = await client.From<CourseModel>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();
            var course = courseResponse.Models.FirstOrDefault();

            if (course == null)
            {
                return NotFound();
            }

            // Optionally, fetch activities for this course
            var activitiesResponse = await client.From<ActivityModel>()
                .Filter("courseId", Supabase.Postgrest.Constants.Operator.Equals, (int)id)
                .Get();
            var activities = activitiesResponse.Models;

            // Map to your view model as needed
            var viewModel = new TeacherCourseViewModel
            {
                Id = (int)course.Id,
                CourseCode = course.Code,
                CourseTitle = course.Name,
                SemesterInfo = $"Level: {course.Level}", // Display course level
                CardColor = "#E8F9E8" // Or fetch from DB if available
            };

            // Pass activities to the view if your view expects them
            ViewBag.Activities = activities;

            return View("Courses/FullCourseView", viewModel);
        }

        [HttpGet]
        public IActionResult CourseStudents(int id)
        {
            // Placeholder
            var course = new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };
            return View("Courses/CourseStudents", course);
        }

        [HttpGet]
        public IActionResult EditCourse(int id)
        {
            // Placeholder
            var course = new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };
            return View("Courses/EditCourse", course);
        }

        // ==================== NEW BACKEND FUNCTIONALITIES ====================

        /// <summary>
        /// Gets available students (not enrolled in the course).
        /// </summary>
        /// <param name="courseId">The course ID.</param>
        /// <returns>JSON result with available students.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAvailableStudents(int courseId)
        {
            try
            {
                Console.WriteLine($"=== GetAvailableStudents START ===");
                Console.WriteLine($"CourseId: {courseId}");
                
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // Get all enrollments for this course (check both "active" and "Active" for case sensitivity)
                var allEnrollmentsResponse = await client
                    .From<EnrollmentModel>()
                    .Where(e => e.CourseId == courseId)
                    .Get();

                var allEnrollments = allEnrollmentsResponse?.Models ?? new List<EnrollmentModel>();
                Console.WriteLine($"Total enrollments for course {courseId}: {allEnrollments.Count}");
                
                // Filter active enrollments (check for "Active" enum value)
                var activeEnrollments = allEnrollments
                    .Where(e => !string.IsNullOrEmpty(e.Status) && 
                                (e.Status == "Active" || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                Console.WriteLine($"Active enrollments: {activeEnrollments.Count}");
                
                var enrolledStudentIds = activeEnrollments
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToList();
                
                Console.WriteLine($"Enrolled student IDs: {string.Join(", ", enrolledStudentIds)}");

                // Get all students
                var allStudents = await _userService.GetStudentsAsync();
                Console.WriteLine($"Total students retrieved: {allStudents.Count}");
                
                if (allStudents.Count == 0)
                {
                    Console.WriteLine("WARNING: No students found in database!");
                    return Json(new { 
                        success = true, 
                        students = new List<object>(),
                        debug = new {
                            totalStudents = 0,
                            enrolledCount = enrolledStudentIds.Count,
                            message = "No students found in database. Please create students first."
                        }
                    });
                }

                // Filter out already enrolled students
                var availableStudents = allStudents
                    .Where(s => !enrolledStudentIds.Contains(s.UserTypeId))
                    .Select(s => new
                    {
                        studentId = s.UserTypeId,
                        idNumber = s.UserDisplayId ?? "N/A",
                        firstName = s.FirstName ?? "",
                        lastName = s.LastName ?? "",
                        status = s.IsActive == true ? "Active" : "Inactive"
                    })
                    .ToList();

                Console.WriteLine($"Available students (not enrolled): {availableStudents.Count}");
                Console.WriteLine($"=== GetAvailableStudents END ===");

                return Json(new { 
                    success = true, 
                    students = availableStudents,
                    debug = new {
                        totalStudents = allStudents.Count,
                        enrolledCount = enrolledStudentIds.Count,
                        availableCount = availableStudents.Count
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available students: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Enrolls a student in a course.
        /// </summary>
        /// <param name="request">The enrollment request containing courseId and studentId.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> EnrollStudent([FromBody] EnrollStudentRequest request)
        {
            try
            {
                Console.WriteLine($"=== EnrollStudent START ===");
                Console.WriteLine($"CourseId: {request?.CourseId}, StudentId: {request?.StudentId}");
                
                if (request == null || string.IsNullOrWhiteSpace(request.StudentId))
                {
                    return Json(new { success = false, message = "Student ID is required." });
                }

                if (request.CourseId <= 0)
                {
                    return Json(new { success = false, message = "Course ID is required." });
                }

                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // Check if student is already enrolled
                // Get all enrollments for this course ONLY (no status filter in DB query)
                Console.WriteLine($"Querying enrollments for course_id = {request.CourseId}");
                var allEnrollmentsResponse = await client
                    .From<EnrollmentModel>()
                    .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, request.CourseId)
                    .Get();

                var allEnrollments = allEnrollmentsResponse?.Models ?? new List<EnrollmentModel>();
                Console.WriteLine($"Found {allEnrollments.Count} total enrollments for course {request.CourseId}");
                
                // Filter in memory for student_id and active status (check for "Active" enum value)
                var activeEnrollment = allEnrollments
                    .FirstOrDefault(e => e.StudentId == request.StudentId && 
                                         !string.IsNullOrEmpty(e.Status) && 
                                         (e.Status == "Active" || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)));

                if (activeEnrollment != null)
                {
                    Console.WriteLine($"Student {request.StudentId} is already enrolled in course {request.CourseId}");
                    return Json(new { success = false, message = "Student is already enrolled in this course." });
                }

                // Create new enrollment
                Console.WriteLine($"Creating new enrollment for student {request.StudentId} in course {request.CourseId}");
                var enrollment = new EnrollmentModel
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId,
                    Status = "Active",  // Enum expects "Active" (capitalized)
                    EnrolledAt = DateTime.UtcNow,
                    DroppedAt = null
                };

                await client.From<EnrollmentModel>().Insert(enrollment);
                Console.WriteLine($"=== EnrollStudent SUCCESS ===");

                // Log audit activity
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                    var allUsers = await _userService.GetAllUsersAsync();
                    var currentUser = allUsers.FirstOrDefault(u => u.UserTypeId == currentUserId);
                    var course = await _courseService.GetCourseByIdAsync(request.CourseId);
                    var student = allUsers.FirstOrDefault(u => u.UserTypeId == request.StudentId);

                    await _auditLogService.LogActivityAsync(
                        userId: currentUserId,
                        userRole: "Teacher",
                        userName: $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                        actionType: "ADD_STUDENT",
                        actionDescription: $"Added student '{($"{student?.FirstName} {student?.LastName}").Trim()}' to course '{course?.Code} - {course?.Name}'",
                        courseId: request.CourseId,
                        courseCode: course?.Code,
                        courseName: course?.Name,
                        studentId: request.StudentId,
                        studentName: $"{student?.FirstName} {student?.LastName}".Trim()
                    );
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Error logging audit activity: {logEx.Message}");
                }

                return Json(new { success = true, message = "Student enrolled successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EnrollStudent ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Drops a student from a course.
        /// </summary>
        /// <param name="courseId">The course ID.</param>
        /// <param name="studentId">The student ID (UUID string).</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> DropStudent(int courseId, string studentId)
        {
            try
            {
                Console.WriteLine($"=== DropStudent START ===");
                Console.WriteLine($"CourseId: {courseId}, StudentId: {studentId}");

                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return Json(new { success = false, message = "Student ID is required." });
                }

                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // Find the enrollment - get all enrollments for course, filter in memory
                Console.WriteLine($"Querying enrollments for course_id = {courseId}");
                var enrollmentResponse = await client.From<EnrollmentModel>()
                    .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, courseId)
                    .Get();

                var allEnrollments = enrollmentResponse?.Models ?? new List<EnrollmentModel>();
                Console.WriteLine($"Found {allEnrollments.Count} total enrollments for course {courseId}");
                
                // Log all enrollments for debugging
                foreach (var e in allEnrollments)
                {
                    Console.WriteLine($"  Enrollment: StudentId={e.StudentId}, Status={e.Status}, CourseId={e.CourseId}");
                }
                
                // First, check if any enrollment exists for this student (regardless of status)
                var anyEnrollment = allEnrollments.FirstOrDefault(e => e.StudentId == studentId);
                if (anyEnrollment == null)
                {
                    Console.WriteLine($"No enrollment found for student {studentId} in course {courseId}");
                    Console.WriteLine($"Looking for studentId: '{studentId}' (length: {studentId?.Length})");
                    Console.WriteLine($"Available studentIds in enrollments: {string.Join(", ", allEnrollments.Select(e => $"'{e.StudentId}'"))}");
                    return Json(new { success = false, message = "Student is not enrolled in this course." });
                }
                
                Console.WriteLine($"Found enrollment for student: Status={anyEnrollment.Status}, StudentId={anyEnrollment.StudentId}");
                
                // Filter in memory for student_id and active status
                var enrollment = allEnrollments
                    .FirstOrDefault(e => e.StudentId == studentId && 
                                       !string.IsNullOrEmpty(e.Status) && 
                                       (e.Status == "Active" || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)));
                
                if (enrollment == null)
                {
                    Console.WriteLine($"Student {studentId} has enrollment but status is '{anyEnrollment.Status}', not Active");
                    return Json(new { success = false, message = $"Student enrollment status is '{anyEnrollment.Status}'. Cannot remove a student that is not actively enrolled." });
                }

                Console.WriteLine($"Found enrollment. Updating status to 'Dropped'");
                // Update enrollment status to "Dropped" (enum expects capitalized)
                enrollment.Status = "Dropped";
                enrollment.DroppedAt = DateTime.UtcNow;
                await enrollment.Update<EnrollmentModel>();

                Console.WriteLine($"=== DropStudent SUCCESS ===");
                // Note: EnrolledCount was removed from CourseModel. 
                // Course enrollment is now tracked through the enrollment table only.

                // Log audit activity
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                    var allUsers = await _userService.GetAllUsersAsync();
                    var currentUser = allUsers.FirstOrDefault(u => u.UserTypeId == currentUserId);
                    var course = await _courseService.GetCourseByIdAsync(courseId);
                    var student = allUsers.FirstOrDefault(u => u.UserTypeId == studentId);

                    await _auditLogService.LogActivityAsync(
                        userId: currentUserId,
                        userRole: "Teacher",
                        userName: $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                        actionType: "REMOVE_STUDENT",
                        actionDescription: $"Removed student '{($"{student?.FirstName} {student?.LastName}").Trim()}' from course '{course?.Code} - {course?.Name}'",
                        courseId: courseId,
                        courseCode: course?.Code,
                        courseName: course?.Name,
                        studentId: studentId,
                        studentName: $"{student?.FirstName} {student?.LastName}".Trim()
                    );
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Error logging audit activity: {logEx.Message}");
                }

                return Json(new { success = true, message = "Student removed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== DropStudent ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retrieves student account details.
        /// </summary>
        /// <param name="studentId">The student ID.</param>
        /// <returns>JSON result with student details.</returns>
        //[HttpGet]
        //public async Task<IActionResult> GetStudentDetails(long studentId)
        //{
        //    try
        //    {
        //        await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
        //        var client = AsiBasecodeDBContext.SupabaseClient;

        //        var userResponse = await client.From<UserModel>()
        //            .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
        //            .Get();

        //        var student = userResponse.Models.FirstOrDefault();
        //        if (student == null)
        //        {
        //            return Json(new { success = false, message = "Student not found." });
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            data = new
        //            {
        //                id = student.Id,
        //                email = student.Email,
        //                firstName = student.FirstName,
        //                lastName = student.LastName,
        //                middleName = student.MiddleName,
        //                studentId = student.StudentId,
        //                program = student.Program,
        //                yearLevel = student.YearLevel,
        //                contactNumber = student.ContactNumber,
        //                isActive = student.IsActive
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}

        /// <summary>
        /// API endpoint to get all recent activities for the current teacher
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRecentActivities()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get all activities for the teacher (limit to 100)
                var activities = await _auditLogService.GetRecentLogsByUserAsync(currentUserId, limit: 100);
                
                var activitiesData = activities.Select(a => new
                {
                    actionDescription = a.ActionDescription,
                    createdAt = a.CreatedAt.Kind == DateTimeKind.Utc ? a.CreatedAt.ToLocalTime() : a.CreatedAt,
                    formattedDate = (a.CreatedAt.Kind == DateTimeKind.Utc ? a.CreatedAt.ToLocalTime() : a.CreatedAt).ToString("MMMM dd, yyyy, hh:mm tt")
                }).ToList();

                return Json(new { success = true, activities = activitiesData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all recent activities: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveActivity(int activityId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                var activityResponse = await client.From<ActivityModel>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, activityId)
                    .Get();

                var activity = activityResponse.Models.FirstOrDefault();
                if (activity == null)
                {
                    return Json(new { success = false, message = "Activity not found." });
                }

                activity.IsVisible = true;
                activity.InvisibleAt = DateTime.UtcNow;
                await activity.Update<ActivityModel>();

                // Log audit activity
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                    var allUsers = await _userService.GetAllUsersAsync();
                    var currentUser = allUsers.FirstOrDefault(u => u.UserTypeId == currentUserId);
                    var course = await _courseService.GetCourseByIdAsync((int)activity.CourseId);

                    await _auditLogService.LogActivityAsync(
                        userId: currentUserId,
                        userRole: "Teacher",
                        userName: $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                        actionType: "ARCHIVE_ACTIVITY",
                        actionDescription: $"Archived activity '{activity.Title}' in course '{course?.Code} - {course?.Name}'",
                        courseId: activity.CourseId,
                        courseCode: course?.Code,
                        courseName: course?.Name,
                        activityId: activityId,
                        activityTitle: activity.Title
                    );
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Error logging audit activity: {logEx.Message}");
                }

                return Json(new { success = true, message = "Activity archived successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Edits a course activity's grade.
        /// </summary>
        /// <param name="gradeId">The grade ID.</param>
        /// <param name="newGrade">The new grade value.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> EditActivityGrade(int gradeId, decimal newGrade)
        {
            try
            {
                // Get the Supabase user ID from claims
                var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(supabaseUserId))
                    return Json(new { success = false, message = "Unauthorized." });

                // Get teacher ID from the user record using Supabase ID
                var teacherIdString = await GetTeacherIdFromSupabaseIdAsync(supabaseUserId);
                if (string.IsNullOrWhiteSpace(teacherIdString) || !int.TryParse(teacherIdString, out int teacherId))
                    return Json(new { success = false, message = "Teacher not found." });

                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                var gradeResponse = await client.From<GradeModel>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, gradeId)
                    .Get();

                var grade = gradeResponse.Models.FirstOrDefault();
                if (grade == null)
                {
                    return Json(new { success = false, message = "Grade not found." });
                }

                grade.Grade = newGrade;
                grade.UpdatedAt = DateTime.UtcNow;
                grade.GradedBy = teacherId;
                await grade.Update<GradeModel>();

                return Json(new { success = true, message = "Grade updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Adds feedback to an activity grade.
        /// </summary>
        /// <param name="gradeId">The grade ID.</param>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="studentId">The student ID.</param>
        /// <param name="feedbackText">The feedback text.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> AddActivityFeedback(int gradeId, int activityId, long studentId, string feedbackText)
        {
            try
            {
                // Get the Supabase user ID from claims
                var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(supabaseUserId))
                    return Json(new { success = false, message = "Unauthorized." });

                // Get teacher ID from the user record using Supabase ID
                var teacherIdString = await GetTeacherIdFromSupabaseIdAsync(supabaseUserId);
                if (string.IsNullOrWhiteSpace(teacherIdString) || !int.TryParse(teacherIdString, out int teacherId))
                    return Json(new { success = false, message = "Teacher not found." });

                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                var feedback = new FeedbackModel
                {
                    GradeId = gradeId,
                    TeacherId = teacherId,
                    StudentId = studentId,
                    ActivityId = activityId,
                    FeedbackText = feedbackText,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<FeedbackModel>().Insert(feedback);

                return Json(new { success = true, message = "Feedback added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Adds or updates a grade for an activity.
        /// </summary>
        /// <param name="studentId">The student ID.</param>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="grade">The grade value.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        //[HttpPost]
        //public async Task<IActionResult> AddOrUpdateGrade(long studentId, int activityId, decimal grade)
        //{
        //    try
        //    {
        //        await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
        //        var client = AsiBasecodeDBContext.SupabaseClient;

        //        // TODO: Get teacher ID from authentication
        //        int teacherId = 1; // Placeholder

        //        // Check if grade already exists
        //        var existingGradeResponse = await client.From<GradeModel>()
        //            .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
        //            .Filter("activity_id", Supabase.Postgrest.Constants.Operator.Equals, activityId)
        //            .Get();

        //        var existingGrade = existingGradeResponse.Models.FirstOrDefault();

        //        if (existingGrade != null)
        //        {
        //            // Update existing grade
        //            existingGrade.Grade = grade;
        //            existingGrade.UpdatedAt = DateTime.UtcNow;
        //            existingGrade.GradedBy = teacherId;
        //            await existingGrade.Update<GradeModel>();
        //            return Json(new { success = true, message = "Grade updated successfully." });
        //        }
        //        else
        //        {
        //            // Create new grade
        //            var newGrade = new GradeModel
        //            {
        //                StudentId = studentId,
        //                ActivityId = activityId,
        //                Grade = grade,
        //                GradedAt = DateTime.UtcNow,
        //                GradedBy = teacherId
        //            };
        //            await client.From<GradeModel>().Insert(newGrade);
                    
        //            // Mark activity as graded if not already
        //            var activityResponse = await client.From<ActivityModel>()
        //                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, activityId)
        //                .Get();
        //            var activity = activityResponse.Models.FirstOrDefault();
        //            if (activity != null && !activity.IsGraded)
        //            {
        //                activity.IsGraded = true;
        //                await activity.Update<ActivityModel>();
        //            }

        //            return Json(new { success = true, message = "Grade added successfully." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}

        /// <summary>
        /// Retrieves grades filtered by various criteria.
        /// </summary>
        /// <param name="courseId">Optional course ID filter.</param>
        /// <param name="activityId">Optional activity ID filter.</param>
        /// <param name="studentId">Optional student ID filter.</param>
        /// <returns>JSON result with filtered grades.</returns>
        [HttpGet]
        public async Task<IActionResult> FilterGrades(int? courseId, int? activityId, long? studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // Get all grades first, then filter in memory
                var gradesResponse = await client.From<GradeModel>().Get();
                var grades = gradesResponse.Models.ToList();

                // Apply filters
                if (studentId.HasValue)
                {
                    grades = grades.Where(g => g.StudentId == studentId.Value).ToList();
                }

                if (activityId.HasValue)
                {
                    grades = grades.Where(g => g.ActivityId == activityId.Value).ToList();
                }

                // If courseId is provided, filter activities by course
                if (courseId.HasValue)
                {
                    var activitiesResponse = await client.From<ActivityModel>()
                        .Filter("courseId", Supabase.Postgrest.Constants.Operator.Equals, courseId.Value)
                        .Get();
                    var activityIds = activitiesResponse.Models.Select(a => a.Id).ToList();
                    grades = grades.Where(g => activityIds.Contains(g.ActivityId)).ToList();
                }

                return Json(new
                {
                    success = true,
                    data = grades.Select(g => new
                    {
                        id = g.Id,
                        studentId = g.StudentId,
                        activityId = g.ActivityId,
                        grade = g.Grade,
                        gradedAt = g.GradedAt,
                        updatedAt = g.UpdatedAt,
                        gradedBy = g.GradedBy
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Edits a course.
        /// </summary>
        /// <param name="id">The course ID.</param>
        /// <param name="model">The course model with updated data.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> EditCourse(int id, TeacherCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Courses/EditCourse", model);
            }
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var courseResponse = await client.From<CourseModel>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();
                var course = courseResponse.Models.FirstOrDefault();
                if (course == null)
                {
                    ModelState.AddModelError("", "Course not found.");
                    return View("Courses/EditCourse", model);
                }
                // Update course fields
                course.Code = model.CourseCode;
                course.Name = model.CourseTitle;
                // You can add more fields as needed
                await course.Update<CourseModel>();
                TempData["SuccessMessage"] = "Course updated successfully.";
                return RedirectToAction("FullCourseView", new { id = course.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View("Courses/EditCourse", model);
            }
        }

        /// <summary>
        /// Retrieves the average grade and count of grades for a course.
        /// </summary>
        /// <param name="courseId">The course ID.</param>
        /// <returns>JSON result with average grade and count.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCourseGrades(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                // Get all activities for the course
                var activitiesResponse = await client.From<ActivityModel>()
                    .Filter("courseId", Supabase.Postgrest.Constants.Operator.Equals, courseId)
                    .Get();
                var activityIds = activitiesResponse.Models.Select(a => a.Id).ToList();
                if (!activityIds.Any())
                {
                    return Json(new { success = true, message = "No activities found for this course.", data = new { average = 0, count = 0 } });
                }
                // Get all grades for these activities
                var gradesResponse = await client.From<GradeModel>().Get();
                var grades = gradesResponse.Models.Where(g => activityIds.Contains(g.ActivityId)).ToList();
                if (!grades.Any())
                {
                    return Json(new { success = true, message = "No grades found for this course.", data = new { average = 0, count = 0 } });
                }
                var average = grades.Average(g => (double)g.Grade);
                var count = grades.Count;
                return Json(new { success = true, data = new { average, count } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CourseGradebook(int id, string selectedStudentId = null)
        {
            var gradebook = await _teacherCourseService.GetCourseGradebookAsync(id);

            if (!string.IsNullOrEmpty(selectedStudentId))
            {
                var detail = await _teacherCourseService.GetStudentGradeDetailAsync(selectedStudentId, id);
                gradebook.SelectedStudentDetail = detail;
            }

            return View("Courses/CourseGradebook", gradebook);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateScore(string studentId, int activityId, int newScore, int courseId)
        {
            var success = await _teacherCourseService.UpdateActivityScoreAsync(studentId, activityId, newScore);
            if (!success) return BadRequest($"Received: studentId={studentId}, activityId={activityId}, newScore={newScore}, courseId={courseId}");

            return RedirectToAction("CourseGradebook", new { id = courseId, selectedStudentId = studentId });
        }




        [HttpGet]
        public async Task<IActionResult> CourseActivities(int id, int? activityId)
        {
            try
            {
                Console.WriteLine($"=== CourseActivities Controller START ===");
                Console.WriteLine($"CourseId: {id}, ActivityId: {activityId}");
                
                // Load all data for the course
                var model = await _activityService.LoadCourseActivityPageAsync(id);
                
                Console.WriteLine($"Model loaded: {model.Students?.Count ?? 0} students, {model.Activities?.Count ?? 0} activities");
                
                // Debug: Log student details
                if (model.Students != null && model.Students.Any())
                {
                    Console.WriteLine($"Students in model:");
                    foreach (var s in model.Students.Take(5))
                    {
                        Console.WriteLine($"  - {s.FirstName} {s.LastName} (ID: {s.Id})");
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: model.Students is NULL or EMPTY!");
                }

                // Determine selected activity: prefer explicit activityId, otherwise pick the first activity if any
                var selectedActivity = activityId.HasValue
                    ? model.Activities.FirstOrDefault(a => a.Id == activityId.Value)
                    : model.Activities.FirstOrDefault();

                ViewBag.SelectedActivity = selectedActivity;

                // Use the resolved selected activity id for submission lookup
                int? selectedActivityId = selectedActivity?.Id;
                Console.WriteLine($"Selected Activity ID: {selectedActivityId}");

                // Build student performance table using the resolved activity id
                // Show ALL enrolled students, even if they haven't submitted
                var studentList = model.Students ?? new List<ASI.Basecode.Services.ServiceModels.TeacherStudentModel>();
                Console.WriteLine($"Building student performance from {studentList.Count} students");
                
                // Convert to dictionary list for better compatibility with Razor dynamic
                var studentPerf = studentList
                    .Select(student =>
                    {
                        var sub = (model.Submissions ?? new List<ASI.Basecode.Services.ServiceModels.TeacherActivitySubmissionModel>())
                            .FirstOrDefault(s =>
                                s.StudentId == student.Id &&
                                s.ActivityId == selectedActivityId
                            );

                        // Debug: Log submission content in controller
                        if (sub != null)
                        {
                            Console.WriteLine($"=== CourseActivities Controller - Building studentPerf ===");
                            Console.WriteLine($"Student: {student.FirstName} {student.LastName}, ActivityId: {selectedActivityId}");
                            Console.WriteLine($"SubmissionContent: {(string.IsNullOrEmpty(sub.SubmissionContent) ? "NULL/EMPTY" : $"Length={sub.SubmissionContent.Length}")}");
                            Console.WriteLine($"Feedback: {(string.IsNullOrEmpty(sub.Feedback) ? "NULL/EMPTY" : $"Length={sub.Feedback.Length}")}");
                        }

                        // Debug: Log grade information
                        if (sub != null)
                        {
                            Console.WriteLine($"Student {student.FirstName} {student.LastName}: Score={sub.Score}, Status={sub.SubmissionStatus}");
                        }
                        else
                        {
                            Console.WriteLine($"Student {student.FirstName} {student.LastName}: No submission found");
                        }

                        return new Dictionary<string, object>
                        {
                            { "Id", student.Id },
                            { "FirstName", student.FirstName ?? "" },
                            { "LastName", student.LastName ?? "" },
                            { "Grade", sub != null ? (int?)sub.Score : null }, // Explicitly cast to int? to preserve 0 values
                            { "Status", sub?.SubmissionStatus ?? "Not submitted" },
                            { "SubmissionContent", sub?.SubmissionContent ?? "" },
                            { "Feedback", sub?.Feedback ?? "" },
                            { "FeedbackDate", sub?.CreatedAt.ToString("MMM dd, yyyy") ?? "" }
                        };
                    }).ToList<object>();

                Console.WriteLine($"Student performance list built: {studentPerf.Count} students");
                if (studentPerf.Any() && studentPerf.First() is Dictionary<string, object> firstStudent)
                {
                    Console.WriteLine($"Sample student: {firstStudent["FirstName"]} {firstStudent["LastName"]}");
                }
                ViewBag.Students = studentPerf;

                return View("Courses/CourseActivities", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading course activities: {ex.Message}");
                ViewBag.Error = "Unable to load course activities. Please try again later.";
                ViewBag.Students = new List<dynamic>(); // Initialize to prevent null reference
                ViewBag.SelectedActivity = null;
                return View("Courses/CourseActivities", new ASI.Basecode.Services.ServiceModels.TeacherCourseModel { CourseId = id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ActivityDetails(int id)
        {
            try
            {
                var activity = await _activityService.GetActivityDetailsAsync(id);
                return View(activity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading activity details: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity(int id, ASI.Basecode.Services.ServiceModels.TeacherActivityModel form)
        {
            try
            {
                form.CourseId = id;
                
                // Fix: Explicitly parse IsVisible from form (handles string "true"/"false")
                var isVisibleRaw = Request.Form["IsVisible"].ToString();
                if (!string.IsNullOrEmpty(isVisibleRaw))
                {
                    form.IsVisible = isVisibleRaw.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                
                // Debug: Log the form values
                Console.WriteLine($"=== CreateActivity Controller ===");
                Console.WriteLine($"Form IsVisible (raw): {isVisibleRaw}");
                Console.WriteLine($"Form IsVisible (parsed bool): {form.IsVisible}");
                
                await _activityService.CreateActivityAsync(form);

                // Log audit activity
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                    var allUsers = await _userService.GetAllUsersAsync();
                    var currentUser = allUsers.FirstOrDefault(u => u.UserTypeId == currentUserId);
                    var course = await _courseService.GetCourseByIdAsync(id);

                    await _auditLogService.LogActivityAsync(
                        userId: currentUserId,
                        userRole: "Teacher",
                        userName: $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                        actionType: "CREATE_ACTIVITY",
                        actionDescription: $"Created activity '{form.Title}' in course '{course?.Code} - {course?.Name}'",
                        courseId: id,
                        courseCode: course?.Code,
                        courseName: course?.Name,
                        activityId: form.Id,
                        activityTitle: form.Title
                    );
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Error logging audit activity: {logEx.Message}");
                }

                return RedirectToAction("CourseActivities", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating activity: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to create activity. Please try again.";
                return RedirectToAction("CourseActivities", new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditActivity(int id)
        {
            try
            {
                var activity = await _activityService.GetActivityDetailsAsync(id);
                return View(activity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading activity for edit: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateActivity(int id, int activityId, ASI.Basecode.Services.ServiceModels.TeacherActivityModel form)
        {
            try
            {
                form.Id = activityId;
                form.CourseId = id;
                
                // Fix: Explicitly parse IsVisible from form (handles string "true"/"false")
                var isVisibleRaw = Request.Form["IsVisible"].ToString();
                if (!string.IsNullOrEmpty(isVisibleRaw))
                {
                    form.IsVisible = isVisibleRaw.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                
                // Debug: Log the form values
                Console.WriteLine($"=== UpdateActivity Controller ===");
                Console.WriteLine($"Form IsVisible (raw): {isVisibleRaw}");
                Console.WriteLine($"Form IsVisible (parsed bool): {form.IsVisible}");
                
                await _activityService.UpdateActivityAsync(form);

                // Log audit activity
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                    var allUsers = await _userService.GetAllUsersAsync();
                    var currentUser = allUsers.FirstOrDefault(u => u.UserTypeId == currentUserId);
                    var course = await _courseService.GetCourseByIdAsync(id);

                    await _auditLogService.LogActivityAsync(
                        userId: currentUserId,
                        userRole: "Teacher",
                        userName: $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                        actionType: "UPDATE_ACTIVITY",
                        actionDescription: $"Updated activity '{form.Title}' in course '{course?.Code} - {course?.Name}'",
                        courseId: id,
                        courseCode: course?.Code,
                        courseName: course?.Name,
                        activityId: activityId,
                        activityTitle: form.Title
                    );
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Error logging audit activity: {logEx.Message}");
                }

                return RedirectToAction("CourseActivities", new { id, activityId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating activity: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update activity. Please try again.";
                return RedirectToAction("CourseActivities", new { id });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Temporarily disable for AJAX testing
        public async Task<IActionResult> GradeActivity(int id, ASI.Basecode.Services.ServiceModels.TeacherActivitySubmissionModel form)
        {
            try
            {
                Console.WriteLine($"=== GradeActivity Controller Called ===");
                Console.WriteLine($"id: {id}, ActivityId: {form?.ActivityId}, StudentId: {form?.StudentId}, Score: {form?.Score}");
                Console.WriteLine($"Feedback: {form?.Feedback ?? "NULL"}");
                
                // Validate model
                if (form == null)
                {
                    Console.WriteLine("ERROR: Form model is null");
                    if (Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Invalid request data" });
                    }
                    TempData["ErrorMessage"] = "Invalid request data.";
                    return RedirectToAction("CourseActivities", new { id });
                }

                if (form.ActivityId == 0 || string.IsNullOrWhiteSpace(form.StudentId))
                {
                    Console.WriteLine($"ERROR: Invalid form data - ActivityId: {form.ActivityId}, StudentId: {form.StudentId}");
                    if (Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Invalid activity or student ID" });
                    }
                    TempData["ErrorMessage"] = "Invalid activity or student ID.";
                    return RedirectToAction("CourseActivities", new { id });
                }

                // Ensure feedback is preserved if not provided (for inline grade editing)
                if (string.IsNullOrWhiteSpace(form.Feedback))
                {
                    // Get existing submission to preserve feedback
                    var existingSubmission = await _activityRepository.GetSubmissionAsync(form.ActivityId, form.StudentId);
                    if (existingSubmission != null && !string.IsNullOrWhiteSpace(existingSubmission.Feedback))
                    {
                        form.Feedback = existingSubmission.Feedback;
                        Console.WriteLine($"Preserved existing feedback (length: {existingSubmission.Feedback.Length})");
                    }
                    else
                    {
                        form.Feedback = string.Empty;
                        Console.WriteLine("No existing feedback found");
                    }
                }

                Console.WriteLine("Calling GradeActivityAsync...");
                await _activityService.GradeActivityAsync(form);
                Console.WriteLine("GradeActivityAsync completed successfully");

                // Log audit activity
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                    var allUsers = await _userService.GetAllUsersAsync();
                    var currentUser = allUsers.FirstOrDefault(u => u.UserTypeId == currentUserId);
                    var course = await _courseService.GetCourseByIdAsync(id);
                    var student = allUsers.FirstOrDefault(u => u.UserTypeId == form.StudentId);
                    var activity = await _activityService.GetActivityDetailsAsync(form.ActivityId);

                    await _auditLogService.LogActivityAsync(
                        userId: currentUserId,
                        userRole: "Teacher",
                        userName: $"{currentUser?.FirstName} {currentUser?.LastName}".Trim(),
                        actionType: "GRADE_ACTIVITY",
                        actionDescription: $"Graded activity '{activity?.Title}' for student '{($"{student?.FirstName} {student?.LastName}").Trim()}' with score {form.Score}",
                        courseId: id,
                        courseCode: course?.Code,
                        courseName: course?.Name,
                        activityId: form.ActivityId,
                        activityTitle: activity?.Title,
                        studentId: form.StudentId,
                        studentName: $"{student?.FirstName} {student?.LastName}".Trim()
                    );
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Error logging audit activity: {logEx.Message}");
                }

                // Check if request is AJAX
                if (Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Grade updated successfully" });
                }

                return RedirectToAction("CourseActivities", new { id, activityId = form.ActivityId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error grading activity: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Check if request is AJAX
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to grade activity. Please try again." });
                }

                TempData["ErrorMessage"] = "Failed to grade activity. Please try again.";
                return RedirectToAction("CourseActivities", new { id });
            }
        }

        /// <summary>
        /// Gets the teacher database ID from the Supabase user ID
        /// </summary>
        private async Task<string> GetTeacherIdFromSupabaseIdAsync(string supabaseUserId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                var userQuery = await client
                    .From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Get();

                var userRecord = userQuery?.Models?.FirstOrDefault();
                return userRecord?.Id.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting teacher ID from Supabase ID: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Request model for enrolling a student in a course.
    /// </summary>
    public class EnrollStudentRequest
    {
        public int CourseId { get; set; }
        public string StudentId { get; set; }
    }
}