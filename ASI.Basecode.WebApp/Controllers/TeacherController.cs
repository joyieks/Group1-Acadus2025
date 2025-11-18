using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;  // ? For ISupabaseAuthService and ICourseService
using ASI.Basecode.WebApp.Models;  // ? For TeacherCourseViewModel
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ASI.Basecode.Service.ServiceModels;

namespace ASI.Basecode.WebApp.Controllers
{
    /// Controller for teacher-related actions and dashboard statistics.
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly ICourseService _courseService;  // ? ADD ICourseService
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly ITeacherCourseActivityService _activityService;



        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// <param name="configuration">Application configuration.</param>
        /// <param name="supabaseAuthService">Supabase authentication service.</param>
        /// <param name="courseService">Course service for database operations.</param>

        public TeacherController(IConfiguration configuration, ISupabaseAuthService supabaseAuthService, ITeacherCourseService teacherCourseService, ICourseService courseService, ITeacherCourseActivityService activityService)
        {
            _configuration = configuration;
            _supabaseAuthService = supabaseAuthService;
            _teacherCourseService = teacherCourseService;
            _courseService = courseService;
            _activityService = activityService;
        }

        /// <summary>
        /// Displays the teacher dashboard with statistics.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            // TODO: Implement actual dashboard logic
            var model = new TeacherDashboardViewModel
            {
                TotalActivities = 10, // Dummy data
                GradedActivities = 5, // Dummy data
                TotalCoursesHandled = 3 // Dummy data
            };
            return View(model);
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

            var roles = User?.Claims?.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
            Console.WriteLine($"[TeacherController.FullCourseView] Request.Path={Request?.Path}, id={id}, IsAuthenticated={User?.Identity?.IsAuthenticated}, Name={User?.Identity?.Name}, Roles={string.Join(',', roles)}");

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
        /// Drops a student from a course.
        /// </summary>
        /// <param name="courseId">The course ID.</param>
        /// <param name="studentId">The student ID.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> DropStudent(int courseId, long studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // Find the enrollment
                var enrollmentResponse = await client.From<EnrollmentModel>()
                    .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, courseId)
                    .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                    .Filter("status", Supabase.Postgrest.Constants.Operator.Equals, "active")
                    .Get();

                var enrollment = enrollmentResponse.Models.FirstOrDefault();
                if (enrollment == null)
                {
                    return Json(new { success = false, message = "Enrollment not found or already dropped." });
                }

                // Update enrollment status to "dropped"
                enrollment.Status = "dropped";
                enrollment.DroppedAt = DateTime.UtcNow;
                await enrollment.Update<EnrollmentModel>();

                // Note: EnrolledCount was removed from CourseModel. 
                // Course enrollment is now tracked through the enrollment table only.

                return Json(new { success = true, message = "Student dropped successfully." });
            }
            catch (Exception ex)
            {
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
        /// Archives a course activity.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <returns>JSON result indicating success or failure.</returns>
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
                activity.Invisible_At = DateTime.UtcNow;
                await activity.Update<ActivityModel>();

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

        // COURSE ACTIVITY HERE
        // GET /Teacher/CourseActivities?id={courseId}&activityId={activityId}
        [HttpGet]
        public async Task<IActionResult> CourseActivities(int id, int? activityId)
        {
            // Load all data for the course
            var model = await _activityService.LoadCourseActivityPageAsync(id);

            // Determine selected activity: prefer explicit activityId, otherwise pick the first activity if any.
            var selectedActivity = activityId.HasValue
                ? model.Activities.FirstOrDefault(a => a.Id == activityId.Value)
                : model.Activities.FirstOrDefault();

            ViewBag.SelectedActivity = selectedActivity;

            // Use the resolved selected activity id for submission lookup (may be null if there are no activities)
            int? selectedActivityId = selectedActivity?.Id;

            // Build student performance table using the resolved activity id
            var studentPerf = model.Students.Select(student =>
            {
                var sub = model.Submissions.FirstOrDefault(s =>
                    s.StudentId == student.Id &&
                    s.ActivityId == selectedActivityId
                );

                return new
                {
                    student.Id,
                    student.FirstName,
                    student.LastName,
                    Grade = sub?.Score,
                    Status = sub?.SubmissionStatus ?? "Not submitted"
                };
            }).ToList();

            ViewBag.Students = studentPerf;

            return View("Courses/CourseActivities", model);
        }

        // VIEW DETAILS
        [HttpGet]
        public async Task<IActionResult> ActivityDetails(int id)
        {
            var activity = await _activityService.GetActivityDetailsAsync(id);
            return View(activity);
        }


        // CREATE ACTIVITY 
        [HttpPost]
        public async Task<IActionResult> CreateActivity(int id, TeacherActivityModel form)
        {
            // UI uses "Name" instead of "Title"
            if (!string.IsNullOrEmpty(form.Title))
                form.Title = form.Title;

            form.CourseId = id;

            await _activityService.CreateActivityAsync(form);

            return RedirectToAction("CourseActivities", new { id });
        }

        // EDIT ACTIVITY 
        [HttpGet]
        public async Task<IActionResult> EditActivity(int id)
        {
            var activity = await _activityService.GetActivityDetailsAsync(id);

            return View(activity);
        }

        [HttpPost]
        //POST /Teacher/UpdateActivity?id={courseId}&activityId={activityId}
        public async Task<IActionResult> UpdateActivity(int id, int activityId, TeacherActivityModel form)
        {
            // UI uses "Name"
            if (!string.IsNullOrEmpty(form.Title))
                form.Title = form.Title;

            form.Id = activityId;
            form.CourseId = id;

            await _activityService.UpdateActivityAsync(form);

            return RedirectToAction("CourseActivities", new { id, activityId });
        }


        [HttpPost]
        //POST /Teacher/GradeActivity?id={courseId}
        public async Task<IActionResult> GradeActivity(int id, TeacherActivitySubmissionModel form)
        {
            await _activityService.GradeActivityAsync(form);

            return RedirectToAction("CourseActivities", new { id, activityId = form.ActivityId });
        }

    }
}
