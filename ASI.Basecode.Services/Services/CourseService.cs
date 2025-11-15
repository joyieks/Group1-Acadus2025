using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class CourseService : ServiceBase, ICourseService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;

        public CourseService(ISupabaseAuthService supabaseAuthService, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _supabaseAuthService = supabaseAuthService;
        }

        /// <summary>
        /// Retrieves all courses from the database.
        /// </summary>
        public async Task<List<CourseModel>> GetAllCoursesAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var coursesQuery = await client
                    .From<CourseModel>()
                    .Get();

                var coursesList = coursesQuery?.Models ?? new List<CourseModel>();

                Console.WriteLine($"Retrieved {coursesList.Count} courses from database");
                return coursesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all courses: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<CourseModel>();
            }
        }

        /// <summary>
        /// Retrieves a specific course by ID.
        /// </summary>
        public async Task<CourseModel> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var courseQuery = await client
                    .From<CourseModel>()
                    .Where(c => c.Id == courseId)
                    .Single();

                Console.WriteLine($"Retrieved course with ID {courseId}");
                return courseQuery;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving course with ID {courseId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves all active courses.
        /// </summary>
        public async Task<List<CourseModel>> GetActiveCoursesAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var coursesQuery = await client
                    .From<CourseModel>()
                    .Get();

                var coursesList = coursesQuery?.Models ?? new List<CourseModel>();

                // Filter for active courses
                var activeCourses = coursesList
                    .Where(c => c.Status == "Active")
                    .ToList();

                Console.WriteLine($"Retrieved {activeCourses.Count} active courses from {coursesList.Count} total courses");
                return activeCourses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving active courses: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<CourseModel>();
            }
        }

        /// <summary>
        /// Retrieves courses for a specific student.
        /// </summary>
        public async Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get student enrollments (you'll need to query the enrollments table)
                // For now, this returns active courses - you may need to join with enrollments table
                var coursesQuery = await client
                    .From<CourseModel>()
                    .Get();

                var coursesList = coursesQuery?.Models ?? new List<CourseModel>();

                Console.WriteLine($"Retrieved {coursesList.Count} courses for student {studentId}");
                return coursesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving courses for student {studentId}: {ex.Message}");
                return new List<CourseModel>();
            }
        }

        /// <summary>
        /// Retrieves detailed information about a course for a student.
        /// </summary>
        public async Task<StudentCourseDetailsViewModel> GetCourseDetailsAsync(string studentId, string courseId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var courseQuery = await client
                    .From<CourseModel>()
                    .Where(c => c.Id.ToString() == courseId)
                    .Single();

                var courseDetails = new StudentCourseDetailsViewModel
                {
                    CourseId = courseQuery.Id.ToString(),
                    CourseTitle = courseQuery.Name,
                    Feedbacks = new List<StudentCourseDetailsViewModel.FeedbackItem>(),
                    Activities = new List<StudentCourseDetailsViewModel.ActivityItem>()
                };

                Console.WriteLine($"Retrieved details for course {courseId}");
                return courseDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving course details for course {courseId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves courses taught by a specific instructor.
        /// </summary>
        public async Task<List<CourseModel>> GetCoursesByInstructorAsync(string instructorId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var coursesQuery = await client
                    .From<CourseModel>()
                    .Get();

                var coursesList = coursesQuery?.Models ?? new List<CourseModel>();

                // Filter courses where TeacherId matches the instructor's ID
                var instructorCourses = coursesList
                    .Where(c => c.TeacherId == instructorId)
                    .ToList();

                Console.WriteLine($"Retrieved {instructorCourses.Count} courses for instructor {instructorId}");
                return instructorCourses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving courses for instructor {instructorId}: {ex.Message}");
                return new List<CourseModel>();
            }
        }

        /// <summary>
        /// Retrieves all active instructors (users with roleId=2 and isActive=true).
        /// </summary>
        public async Task<List<(string UserTypeId, string FullName)>> GetActiveInstructorsAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get all user roles with roleId = 2 (instructor)
                var userRolesQuery = await client
                    .From<UserRole>()
                    .Where(ur => ur.RoleId == 2)
                    .Get();

                var userRolesList = userRolesQuery?.Models ?? new List<UserRole>();
                Console.WriteLine($"Found {userRolesList.Count} instructor role assignments");

                // Get all users
                var usersQuery = await client
                    .From<SupabaseUserNew>()
                    .Get();

                var usersList = usersQuery?.Models ?? new List<SupabaseUserNew>();
                Console.WriteLine($"Found {usersList.Count} total users");

                // Filter for active instructors: roleId=2 AND isActive=true
                var activeInstructors = new List<(string, string)>();

                foreach (var userRole in userRolesList)
                {
                    var user = usersList.FirstOrDefault(u => u.UserTypeId == userRole.UserId && u.IsActive == true);
                    if (user != null)
                    {
                        var fullName = $"{user.FirstName} {user.LastName}".Trim();
                        activeInstructors.Add((user.UserTypeId, fullName));
                        Console.WriteLine($"Added instructor: {fullName} (UserTypeId: {user.UserTypeId})");
                    }
                }

                Console.WriteLine($"Retrieved {activeInstructors.Count} active instructors");
                return activeInstructors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving active instructors: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<(string, string)>();
            }
        }

        /// <summary>
        /// Retrieves all semesters from the database.
        /// </summary>
        public async Task<List<SemesterModel>> GetAllSemestersAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var semestersQuery = await client
                    .From<SemesterModel>()
                    .Get();

                var semestersList = semestersQuery?.Models ?? new List<SemesterModel>();
                Console.WriteLine($"Retrieved {semestersList.Count} semesters");
                return semestersList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving semesters: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SemesterModel>();
            }
        }

        /// <summary>
        /// Creates a new course with validation.
        /// </summary>
        public async Task<(bool Success, string Message, int? CourseId)> CreateCourseAsync(
            string code,
            string name, 
            string description,
            long credits,
            string level,
            long semesterId, 
            decimal maxCapacity, 
            string instructorId,
            string status = "Active")
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(code) || code.Length < 2 || code.Length > 50)
                {
                    return (false, "Course code must be between 2 and 50 characters", null);
                }

                if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 255)
                {
                    return (false, "Course name must be between 3 and 255 characters", null);
                }

                if (string.IsNullOrWhiteSpace(description) || description.Length < 10 || description.Length > 1000)
                {
                    return (false, "Course description must be between 10 and 1000 characters", null);
                }

                if (credits < 1 || credits > 6)
                {
                    return (false, "Credits must be between 1 and 6", null);
                }

                if (string.IsNullOrWhiteSpace(level))
                {
                    return (false, "Course level is required", null);
                }

                if (semesterId <= 0)
                {
                    return (false, "Valid semester is required", null);
                }

                if (maxCapacity < 1 || maxCapacity > 500)
                {
                    return (false, "Maximum capacity must be between 1 and 500", null);
                }

                if (string.IsNullOrWhiteSpace(instructorId))
                {
                    return (false, "Instructor is required", null);
                }

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Check if course code already exists
                var existingCourseQuery = await client
                    .From<CourseModel>()
                    .Where(c => c.Code == code)
                    .Get();

                var existingCourses = existingCourseQuery?.Models ?? new List<CourseModel>();
                if (existingCourses.Count > 0)
                {
                    return (false, $"Course code '{code}' already exists", null);
                }

                // Create new course
                var newCourse = new CourseModel
                {
                    Code = code,
                    Name = name,
                    Description = description,
                    Credits = credits,
                    Level = level,
                    SemesterId = semesterId,
                    MaxCapacity = maxCapacity,
                    TeacherId = instructorId,
                    Status = status
                };

                var result = await client
                    .From<CourseModel>()
                    .Insert(newCourse);

                Console.WriteLine($"Course created successfully: {name} ({code})");
                return (true, "Course created successfully", (int)newCourse.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating course: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (false, $"Error creating course: {ex.Message}", null);
            }
        }
    }
}