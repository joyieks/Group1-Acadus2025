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
        /// Searches for courses by code or name.
        /// </summary>
        public async Task<List<CourseModel>> SearchCoursesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllCoursesAsync();
                }

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                var allCourses = await GetAllCoursesAsync();

                // Filter courses by code or name (case-insensitive)
                var searchLower = searchTerm.ToLower();
                var filteredCourses = allCourses
                    .Where(c => 
                        (c.Code != null && c.Code.ToLower().Contains(searchLower)) ||
                        (c.Name != null && c.Name.ToLower().Contains(searchLower))
                    )
                    .ToList();

                Console.WriteLine($"Search for '{searchTerm}' returned {filteredCourses.Count} courses");
                return filteredCourses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching courses: {ex.Message}");
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
        /// Generates a unique course code based on year level.
        /// Format: [Year Level Prefix][3-digit Index]
        /// 1st Year: 141, 2nd Year: 242, 3rd Year: 626, 4th Year: 919
        /// </summary>
        public async Task<string> GenerateCourseCodeAsync(string level)
        {
            try
            {
                // Map year level to prefix
                var levelPrefixMap = new Dictionary<string, string>
                {
                    { "1st Year", "141" },
                    { "2nd Year", "242" },
                    { "3rd Year", "626" },
                    { "4th Year", "919" }
                };

                if (!levelPrefixMap.ContainsKey(level))
                {
                    throw new ArgumentException($"Invalid year level: {level}");
                }

                var prefix = levelPrefixMap[level];
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get all courses with codes starting with this prefix
                var allCourses = await client
                    .From<CourseModel>()
                    .Get();

                var courses = allCourses?.Models ?? new List<CourseModel>();
                
                // Filter courses that start with the prefix and extract their indices
                var indices = new List<int>();
                foreach (var course in courses)
                {
                    if (!string.IsNullOrEmpty(course.Code) && course.Code.StartsWith(prefix) && course.Code.Length == 6)
                    {
                        // Extract the 3-digit index (last 3 characters)
                        if (int.TryParse(course.Code.Substring(3), out int index))
                        {
                            indices.Add(index);
                        }
                    }
                }

                // Find the next available index
                int nextIndex = 1;
                if (indices.Count > 0)
                {
                    nextIndex = indices.Max() + 1;
                }

                // Ensure index doesn't exceed 999 (3 digits max)
                if (nextIndex > 999)
                {
                    throw new Exception($"Maximum course limit reached for {level}. Cannot generate more course codes.");
                }

                // Generate code: prefix + 3-digit index (padded with zeros)
                var generatedCode = $"{prefix}{nextIndex:D3}";

                Console.WriteLine($"Generated course code: {generatedCode} for {level} (index: {nextIndex})");
                return generatedCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating course code: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new course with validation and auto-generated course code.
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
                // Auto-generate course code if not provided or empty
                if (string.IsNullOrWhiteSpace(code))
                {
                    code = await GenerateCourseCodeAsync(level);
                    Console.WriteLine($"Auto-generated course code: {code}");
                }

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

                if (maxCapacity < 1 || maxCapacity > 50)
                {
                    return (false, "Maximum capacity must be between 1 and 50", null);
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

        /// <summary>
        /// Retrieves all active enrollments for a specific course with student details.
        /// </summary>
        public async Task<List<EnrollmentModel>> GetCourseEnrollmentsByCourseIdAsync(long courseId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get all active enrollments for this course
                var enrollmentsQuery = await client
                    .From<EnrollmentModel>()
                    .Where(e => e.CourseId == courseId && e.Status == "Active")
                    .Get();

                var enrollments = enrollmentsQuery?.Models ?? new List<EnrollmentModel>();

                Console.WriteLine($"Retrieved {enrollments.Count} active enrollments for course {courseId}");
                return enrollments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving enrollments for course {courseId}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<EnrollmentModel>();
            }
        }

        /// <summary>
        /// Updates an existing course with validation.
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateCourseAsync(
            int courseId,
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
                Console.WriteLine($"Starting course update for course ID: {courseId}");

                // Validate input
                if (courseId <= 0)
                {
                    return (false, "Invalid course ID");
                }

                if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 255)
                {
                    return (false, "Course name must be between 3 and 255 characters");
                }

                if (string.IsNullOrWhiteSpace(description) || description.Length < 10 || description.Length > 1000)
                {
                    return (false, "Course description must be between 10 and 1000 characters");
                }

                if (credits < 1 || credits > 6)
                {
                    return (false, "Credits must be between 1 and 6");
                }

                if (string.IsNullOrWhiteSpace(level))
                {
                    return (false, "Course level is required");
                }

                if (semesterId <= 0)
                {
                    return (false, "Valid semester is required");
                }

                if (maxCapacity < 1 || maxCapacity > 50)
                {
                    return (false, "Maximum capacity must be between 1 and 50");
                }

                if (string.IsNullOrWhiteSpace(instructorId))
                {
                    return (false, "Instructor is required");
                }

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Verify course exists
                var existingCourse = await GetCourseByIdAsync(courseId);
                if (existingCourse == null)
                {
                    return (false, $"Course with ID {courseId} not found");
                }

                // Update the course
                var updatedCourse = new CourseModel
                {
                    Id = courseId,
                    Code = existingCourse.Code, // Keep original code
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
                    .Update(updatedCourse);

                Console.WriteLine($"Course updated successfully: {name}");
                return (true, "Course updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating course: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (false, $"Error updating course: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all students not enrolled in a specific course.
        /// Filters by checking course_enrollment for this courseId using userTypeId.
        /// </summary>
        public async Task<List<SupabaseUserNew>> GetAvailableStudentsForCourseAsync(long courseId, string searchTerm = "")
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get all students (active users)
                var allStudentsQuery = await client
                    .From<SupabaseUserNew>()
                    .Where(u => u.IsActive == true)
                    .Get();

                var students = allStudentsQuery?.Models ?? new List<SupabaseUserNew>();
                Console.WriteLine($"Retrieved {students.Count} total active students");

                // Get current enrollments for this course
                var enrollmentsQuery = await client
                    .From<EnrollmentModel>()
                    .Where(e => e.CourseId == courseId)
                    .Get();

                var enrolledStudentIds = enrollmentsQuery?.Models?.Select(e => e.StudentId).ToHashSet() ?? new HashSet<string>();
                Console.WriteLine($"Course {courseId} has {enrolledStudentIds.Count} enrolled students");

                // Filter out already enrolled students
                var availableStudents = students
                    .Where(s => !enrolledStudentIds.Contains(s.UserTypeId))
                    .ToList();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    availableStudents = availableStudents
                        .Where(s => 
                            (s.UserDisplayId != null && s.UserDisplayId.ToLower().Contains(searchLower)) ||
                            (s.FirstName != null && s.FirstName.ToLower().Contains(searchLower)) ||
                            (s.LastName != null && s.LastName.ToLower().Contains(searchLower))
                        )
                        .ToList();
                }

                Console.WriteLine($"Available students for course {courseId}: {availableStudents.Count}");
                return availableStudents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available students for course {courseId}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SupabaseUserNew>();
            }
        }

        /// <summary>
        /// Enrolls a student in a course with validation.
        /// Checks for duplicates, max capacity, and student existence.
        /// </summary>
        public async Task<(bool Success, string Message)> EnrollStudentInCourseAsync(long courseId, string studentId)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Validate course exists
                var course = await GetCourseByIdAsync((int)courseId);
                if (course == null)
                {
                    return (false, "Course not found");
                }

                // Check if student exists
                var studentQuery = await client
                    .From<SupabaseUserNew>()
                    .Where(u => u.UserTypeId == studentId)
                    .Single();

                if (studentQuery == null)
                {
                    return (false, "Student not found");
                }

                // Check if student is already enrolled
                var existingEnrollmentQuery = await client
                    .From<EnrollmentModel>()
                    .Where(e => e.CourseId == courseId && e.StudentId == studentId)
                    .Single();

                if (existingEnrollmentQuery != null)
                {
                    return (false, "Student is already enrolled in this course");
                }

                // Check max capacity
                var currentEnrollmentsQuery = await client
                    .From<EnrollmentModel>()
                    .Where(e => e.CourseId == courseId)
                    .Get();

                var enrollmentCount = currentEnrollmentsQuery?.Models?.Count ?? 0;
                if (enrollmentCount >= course.MaxCapacity)
                {
                    return (false, $"Course is at maximum capacity ({course.MaxCapacity} students)");
                }

                // Create new enrollment
                var newEnrollment = new EnrollmentModel
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow,
                    Status = "active"
                };

                await client
                    .From<EnrollmentModel>()
                    .Insert(newEnrollment);

                Console.WriteLine($"Student {studentId} enrolled in course {courseId}");
                return (true, "Student enrolled successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enrolling student {studentId} in course {courseId}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (false, $"Error enrolling student: {ex.Message}");
            }
        }
    }
}