using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ASI.Basecode.WebApp.Models;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for teacher-related actions and dashboard statistics.
    /// </summary>
    public class TeacherController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public TeacherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the teacher dashboard with statistics.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            var client = AsiBasecodeDBContext.SupabaseClient;

            // TODO: Replace with actual teacher ID from authentication
            int teacherId = 1; // Example teacher ID

            // Get total activities for the teacher
            var activitiesResponse = await client.From<ActivityModel>()
                .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                .Get();
            var activities = activitiesResponse.Models;
            int totalActivities = activities.Count;

            // Get graded activities for the teacher
            int gradedActivities = activities.Count(a => a.IsGraded);

            // Get total courses handled by the teacher
            var coursesResponse = await client.From<CourseModel>()
                .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                .Get();
            var courses = coursesResponse.Models;
            int totalCoursesHandled = courses.Count;

            // TODO: Implement calendar events retrieval if needed
            var calendarEvents = new List<string>();

            var model = new TeacherDashboardViewModel
            {
                TotalActivities = totalActivities,
                GradedActivities = gradedActivities,
                TotalCoursesHandled = totalCoursesHandled,
                CalendarEvents = calendarEvents
            };
            return View(model);
        }

        /// <summary>
        /// Displays the teacher's courses view.
        /// </summary>
        /// <returns>The courses view.</returns>
        [HttpGet]
        public IActionResult Courses()
        {
            var courses = new List<TeacherCourseViewModel>
            {
                new TeacherCourseViewModel
                {
                    Id = 1,
                    CourseCode = "91299 - ELPHP41",
                    CourseTitle = "FREE ELECTIVE - PHP",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    Id = 2,
                    CourseCode = "91300 - CS101",
                    CourseTitle = "INTRODUCTION TO COMPUTER SCIENCE",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    Id = 3,
                    CourseCode = "91301 - MATH201",
                    CourseTitle = "DISCRETE MATHEMATICS",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    Id = 4,
                    CourseCode = "91302 - ENG102",
                    CourseTitle = "TECHNICAL WRITING",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#6EE7B7"
                },
                new TeacherCourseViewModel
                {
                    Id = 5,
                    CourseCode = "91303 - DATA301",
                    CourseTitle = "DATA STRUCTURES",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#34D399"
                },
                new TeacherCourseViewModel
                {
                    Id = 6,
                    CourseCode = "91304 - WEBDEV401",
                    CourseTitle = "WEB DEVELOPMENT",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#10B981"
                }
            };

            return View("Courses/Index", courses.ToArray());
        }

        /// <summary>
        /// Displays the full course view.
        /// </summary>
        /// <returns>The full course view.</returns>
        [HttpGet]
        public IActionResult FullCourseView(int id)
        {
            // Placeholder: Find course by id from sample data
            var courses = new List<TeacherCourseViewModel>
            {
                new TeacherCourseViewModel
                {
                    Id = 1,
                    CourseCode = "91299 - ELPHP41",
                    CourseTitle = "FREE ELECTIVE - PHP",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#E8F9E8"
                },
                new TeacherCourseViewModel
                {
                    Id = 2,
                    CourseCode = "91300 - CS101",
                    CourseTitle = "INTRODUCTION TO COMPUTER SCIENCE",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#D1FAE5"
                },
                new TeacherCourseViewModel
                {
                    Id = 3,
                    CourseCode = "91301 - MATH201",
                    CourseTitle = "DISCRETE MATHEMATICS",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#A7F3D0"
                },
                new TeacherCourseViewModel
                {
                    Id = 4,
                    CourseCode = "91302 - ENG102",
                    CourseTitle = "TECHNICAL WRITING",
                    SemesterInfo = "1st Semester 2025 - 2026",
                    CardColor = "#6EE7B7"
                },
                new TeacherCourseViewModel
                {
                    Id = 5,
                    CourseCode = "91303 - DATA301",
                    CourseTitle = "DATA STRUCTURES",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#34D399"
                },
                new TeacherCourseViewModel
                {
                    Id = 6,
                    CourseCode = "91304 - WEBDEV401",
                    CourseTitle = "WEB DEVELOPMENT",
                    SemesterInfo = "2nd Semester 2025 - 2026",
                    CardColor = "#10B981"
                }
            };
            var course = courses.FirstOrDefault(c => c.Id == id) ?? new TeacherCourseViewModel { Id = id, CourseTitle = "Sample Course" };
            return View("Courses/FullCourseView", course);
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

                // Update course enrolled count
                var courseResponse = await client.From<CourseModel>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, courseId)
                    .Get();
                var course = courseResponse.Models.FirstOrDefault();
                if (course != null)
                {
                    course.EnrolledCount = Math.Max(0, course.EnrolledCount - 1);
                    await course.Update<CourseModel>();
                }

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
        [HttpGet]
        public async Task<IActionResult> GetStudentDetails(long studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                var userResponse = await client.From<UserModel>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                    .Get();

                var student = userResponse.Models.FirstOrDefault();
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found." });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = student.Id,
                        email = student.Email,
                        firstName = student.FirstName,
                        lastName = student.LastName,
                        middleName = student.MiddleName,
                        studentId = student.StudentId,
                        program = student.Program,
                        yearLevel = student.YearLevel,
                        contactNumber = student.ContactNumber,
                        isActive = student.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

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

                activity.IsArchived = true;
                activity.ArchivedAt = DateTime.UtcNow;
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
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // TODO: Get teacher ID from authentication
                int teacherId = 1; // Placeholder

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
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // TODO: Get teacher ID from authentication
                int teacherId = 1; // Placeholder

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
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateGrade(long studentId, int activityId, decimal grade)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;

                // TODO: Get teacher ID from authentication
                int teacherId = 1; // Placeholder

                // Check if grade already exists
                var existingGradeResponse = await client.From<GradeModel>()
                    .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                    .Filter("activity_id", Supabase.Postgrest.Constants.Operator.Equals, activityId)
                    .Get();

                var existingGrade = existingGradeResponse.Models.FirstOrDefault();

                if (existingGrade != null)
                {
                    // Update existing grade
                    existingGrade.Grade = grade;
                    existingGrade.UpdatedAt = DateTime.UtcNow;
                    existingGrade.GradedBy = teacherId;
                    await existingGrade.Update<GradeModel>();
                    return Json(new { success = true, message = "Grade updated successfully." });
                }
                else
                {
                    // Create new grade
                    var newGrade = new GradeModel
                    {
                        StudentId = studentId,
                        ActivityId = activityId,
                        Grade = grade,
                        GradedAt = DateTime.UtcNow,
                        GradedBy = teacherId
                    };
                    await client.From<GradeModel>().Insert(newGrade);
                    
                    // Mark activity as graded if not already
                    var activityResponse = await client.From<ActivityModel>()
                        .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, activityId)
                        .Get();
                    var activity = activityResponse.Models.FirstOrDefault();
                    if (activity != null && !activity.IsGraded)
                    {
                        activity.IsGraded = true;
                        await activity.Update<ActivityModel>();
                    }

                    return Json(new { success = true, message = "Grade added successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

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
                        .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, courseId.Value)
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
    }
}
