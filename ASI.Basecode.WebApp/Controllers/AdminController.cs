using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]  // ? CRITICAL: Require authentication and Admin role

    public class AdminController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IAdminService _adminService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public AdminController(
            IStudentService studentService,
            ITeacherService teacherService,
            ISupabaseAuthService supabaseAuthService,
            IAdminService adminService,
            ICourseService courseService,
            IUserService userService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _supabaseAuthService = supabaseAuthService;
            _adminService = adminService;
            _courseService = courseService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var (totalStudents, totalInstructors, totalCourses) = await _adminService.GetDashboardStatisticsAsync();

            Console.WriteLine($"=== AdminController.Dashboard ===");
            Console.WriteLine($"Received from service - Students: {totalStudents}, Instructors: {totalInstructors}, Courses: {totalCourses}");

            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses
            };

            Console.WriteLine($"ViewModel created - Students: {viewModel.TotalStudents}, Instructors: {viewModel.TotalInstructors}, Courses: {viewModel.TotalCourses}");
            Console.WriteLine($"=== End Dashboard ===");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users(string tab = "all", string search = null)
        {
            try
            {
                var (totalStudents, totalInstructors, totalCourses) = await _adminService.GetDashboardStatisticsAsync();

                Console.WriteLine($"=== AdminController.Users ===");
                Console.WriteLine($"Tab: {tab}, Search: {search ?? "none"}");

                // Fetch users based on search and tab
                List<SupabaseUserNew> allUsers;
                List<SupabaseUserNew> students;
                List<SupabaseUserNew> instructors;
                List<SupabaseUserNew> displayedUsers;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    // Search across all users
                    allUsers = await _userService.SearchUsersAsync(search, null);
                    students = await _userService.SearchUsersAsync(search, "1");
                    instructors = await _userService.SearchUsersAsync(search, "2");
                }
                else
                {
                    // Fetch all users
                    allUsers = await _userService.GetAllUsersAsync();
                    students = await _userService.GetStudentsAsync();
                    instructors = await _userService.GetInstructorsAsync();
                }

                // Determine which list to display based on tab
                displayedUsers = tab switch
                {
                    "students" => students,
                    "instructors" => instructors,
                    _ => allUsers
                };

                // Resolve roles for displayed users using user_roles join -> roles
                // Note: Pass users.id (converted to string) not userTypeId
                var displayedWithRoles = new List<UserWithRoleViewModel>();
                foreach (var u in displayedUsers)
                {
                    var rolesForUser = await _userService.GetUserRolesAsync(u.Id.ToString());
                    displayedWithRoles.Add(new UserWithRoleViewModel
                    {
                        User = u,
                        Roles = rolesForUser
                    });
                }

                var viewModel = new UsersTableViewModel
                {
                    AllUsers = allUsers,
                    Students = students,
                    Instructors = instructors,
                    DisplayedUsers = displayedUsers,
                    DisplayedUsersWithRoles = displayedWithRoles,
                    SearchTerm = search,
                    ActiveTab = tab,
                    TotalStudents = totalStudents,
                    TotalInstructors = totalInstructors
                };

                Console.WriteLine($"ViewModel created - All: {allUsers.Count}, Students: {students.Count}, Instructors: {instructors.Count}, Displayed: {displayedUsers.Count}");
                Console.WriteLine($"=== End Users ===");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return View(new UsersTableViewModel());
            }
        }

        // GET: Display the empty form
        [HttpGet]
        public IActionResult AddStudent()
        {
            return View(new StudentCreateDto());
        }

        // POST: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentCreateDto model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var success = await _userService.CreateStudentAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create student. Please try again.");
                    return View(model);
                }

                TempData["SuccessMessage"] = $"Student {model.FirstName} {model.LastName} created successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading programs/departments: {ex.Message}");
                ViewBag.Programs = new List<ASI.Basecode.Data.Models.Program>();
                ViewBag.Departments = new List<Department>();
            }

            return View(new StudentCreateDto());
        }

        [HttpGet]
        public async Task<IActionResult> AddTeacher()
        {
            // ? Load departments from database for dropdown
            try
            {
                var departments = await _adminService.GetAllDepartmentsAsync();

                ViewBag.Departments = departments;

                Console.WriteLine($"Loaded {departments.Count} departments for Add Teacher form");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading departments: {ex.Message}");
                ViewBag.Departments = new List<Department>();
            }

            return View(new TeacherCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(TeacherCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ? FIX: Reload dropdown data when validation fails
                try
                {
                    var departments = await _adminService.GetAllDepartmentsAsync();
                    ViewBag.Departments = departments;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reloading departments: {ex.Message}");
                    ViewBag.Departments = new List<Department>();
                }

                return View(model);
            }

            try
            {
                // Map ViewModel to DTO
                var teacherDto = new TeacherCreateDto
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Suffix = model.Suffix,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    HouseNumber = model.HouseNumber,
                    StreetName = model.StreetName,
                    Subdivision = model.Subdivision,
                    Barangay = model.Barangay,
                    City = model.City,
                    Province = model.Province,
                    ZipCode = model.ZipCode,
                    DepartmentId = model.DepartmentId
                };

                var success = await _userService.CreateTeacherAsync(teacherDto);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Teacher {model.FirstName} {model.LastName} has been successfully created!";
                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create teacher. Please try again.");

                    // ? FIX: Reload dropdown data before returning view
                    try
                    {
                        var departments = await _adminService.GetAllDepartmentsAsync();
                        ViewBag.Departments = departments;
                    }
                    catch (Exception reloadEx)
                    {
                        Console.WriteLine($"Error reloading departments: {reloadEx.Message}");
                        ViewBag.Departments = new List<Department>();
                    }

                    return View(model);
                }
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating teacher: {ex.Message}");

                // ? FIX: Reload dropdown data before returning view
                try
                {
                    var departments = await _adminService.GetAllDepartmentsAsync();
                    ViewBag.Departments = departments;
                }
                catch (Exception reloadEx)
                {
                    Console.WriteLine($"Error reloading departments: {reloadEx.Message}");
                    ViewBag.Departments = new List<Department>();
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Courses(string search)
        {
            try
            {
                List<CourseModel> courses;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    courses = await _courseService.SearchCoursesAsync(search);
                    ViewData["SearchTerm"] = search;
                    Console.WriteLine($"=== AdminController.Courses (Search) ===");
                    Console.WriteLine($"Search term: '{search}'");
                }
                else
                {
                    courses = await _courseService.GetAllCoursesAsync();
                    Console.WriteLine($"=== AdminController.Courses ===");
                }

                Console.WriteLine($"Retrieved {courses.Count} courses");
                return View(courses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving courses: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return View(new List<CourseModel>());
            }
        }

        [HttpGet]
        public IActionResult ViewUser(string id)
        {
            // TODO: Load user data by id
            return View();
        }

        [HttpGet]
        public IActionResult ViewTeacher(string id)
        {
            // TODO: Load teacher data by id
            return View();
        }

        [HttpGet]
        public IActionResult EditUser(string id)
        {
            // TODO: Load user data by id
            return View();
        }

        [HttpGet]
        public IActionResult RecentActivity()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PendingTasks()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            return View();
        }

        /// <summary>
        /// API endpoint to generate course code based on year level
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateCourseCode(string level)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(level))
                {
                    return Json(new { success = false, message = "Year level is required" });
                }

                var generatedCode = await _courseService.GenerateCourseCodeAsync(level);
                return Json(new { success = true, code = generatedCode });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating course code: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddCourse()
        {
            try
            {
                var model = new CourseCreateViewModel();
                model.Code = string.Empty; // Ensure Code is empty for auto-generation

                // Populate instructor dropdown
                var instructors = await _courseService.GetActiveInstructorsAsync();
                model.Instructors = instructors
                    .Select(i => new InstructorOption { UserTypeId = i.UserTypeId, FullName = i.FullName })
                    .ToList();

                // Populate semester dropdown
                var semesters = await _courseService.GetAllSemestersAsync();
                model.Semesters = semesters
                    .Select(s => new SemesterOption { Id = s.Id, SemesterName = s.SemesterName })
                    .ToList();

                // Populate level dropdown
                model.Levels = new List<LevelOption>
                {
                    new LevelOption { Value = "1st Year", Label = "1st Year" },
                    new LevelOption { Value = "2nd Year", Label = "2nd Year" },
                    new LevelOption { Value = "3rd Year", Label = "3rd Year" },
                    new LevelOption { Value = "4th Year", Label = "4th Year" }
                };

                Console.WriteLine($"=== AdminController.AddCourse (GET) ===");
                Console.WriteLine($"Instructors: {model.Instructors.Count}, Semesters: {model.Semesters.Count}, Levels: {model.Levels.Count}");

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading AddCourse form: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading course creation form. Please try again.";
                return RedirectToAction("Courses");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(CourseCreateViewModel model)
        {
            try
            {
                // Remove Code validation errors since it's auto-generated
                ModelState.Remove(nameof(model.Code));

                if (!ModelState.IsValid)
                {
                    // Repopulate dropdowns on validation failure
                    var instructors = await _courseService.GetActiveInstructorsAsync();
                    model.Instructors = instructors
                        .Select(i => new InstructorOption { UserTypeId = i.UserTypeId, FullName = i.FullName })
                        .ToList();

                    var semesters = await _courseService.GetAllSemestersAsync();
                    model.Semesters = semesters
                        .Select(s => new SemesterOption { Id = s.Id, SemesterName = s.SemesterName })
                        .ToList();

                    model.Levels = new List<LevelOption>
                    {
                        new LevelOption { Value = "1st Year", Label = "1st Year" },
                        new LevelOption { Value = "2nd Year", Label = "2nd Year" },
                        new LevelOption { Value = "3rd Year", Label = "3rd Year" },
                        new LevelOption { Value = "4th Year", Label = "4th Year" }
                    };

                    return View(model);
                }

                Console.WriteLine($"=== AdminController.AddCourse (POST) ===");
                Console.WriteLine($"Creating course: {model.Name} (Code will be auto-generated)");

                // Pass empty code to trigger auto-generation
                var (success, message, courseId) = await _courseService.CreateCourseAsync(
                    code: string.Empty, // Auto-generate code
                    name: model.Name,
                    description: model.Description,
                    credits: model.Credits,
                    level: model.Level,
                    semesterId: model.SemesterId,
                    maxCapacity: model.MaxCapacity,
                    instructorId: model.InstructorId,
                    status: model.Status
                );

                if (success)
                {
                    TempData["SuccessMessage"] = $"Course '{model.Name}' has been created successfully!";
                    return RedirectToAction("Courses");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);

                    // Repopulate dropdowns
                    var instructors = await _courseService.GetActiveInstructorsAsync();
                    model.Instructors = instructors
                        .Select(i => new InstructorOption { UserTypeId = i.UserTypeId, FullName = i.FullName })
                        .ToList();

                    var semesters = await _courseService.GetAllSemestersAsync();
                    model.Semesters = semesters
                        .Select(s => new SemesterOption { Id = s.Id, SemesterName = s.SemesterName })
                        .ToList();

                    model.Levels = new List<LevelOption>
                    {
                        new LevelOption { Value = "1st Year", Label = "1st Year" },
                        new LevelOption { Value = "2nd Year", Label = "2nd Year" },
                        new LevelOption { Value = "3rd Year", Label = "3rd Year" },
                        new LevelOption { Value = "4th Year", Label = "4th Year" }
                    };

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating course: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"Error creating course: {ex.Message}");

                // Repopulate dropdowns on exception
                var instructors = await _courseService.GetActiveInstructorsAsync();
                model.Instructors = instructors
                    .Select(i => new InstructorOption { UserTypeId = i.UserTypeId, FullName = i.FullName })
                    .ToList();

                var semesters = await _courseService.GetAllSemestersAsync();
                model.Semesters = semesters
                    .Select(s => new SemesterOption { Id = s.Id, SemesterName = s.SemesterName })
                    .ToList();

                model.Levels = new List<LevelOption>
                {
                    new LevelOption { Value = "1st Year", Label = "1st Year" },
                    new LevelOption { Value = "2nd Year", Label = "2nd Year" },
                    new LevelOption { Value = "3rd Year", Label = "3rd Year" },
                    new LevelOption { Value = "4th Year", Label = "4th Year" }
                };

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewCourse(string id, string search)
        {
            try
            {
                // Parse the ID to int
                if (!int.TryParse(id, out int courseId)) return NotFound();

                // Fetch the course data from the service
                var course = await _courseService.GetCourseByIdAsync(courseId);

                if (course == null) return NotFound();

                // Fetch enrolled students for this course
                var enrollments = await _courseService.GetCourseEnrollmentsByCourseIdAsync(courseId);
                Console.WriteLine($"Retrieved {enrollments.Count} enrollments for course {courseId}");

                // Build the enrolled students list with user details
                var enrolledStudents = new List<CourseEnrolledStudentViewModel>();

                foreach (var enrollment in enrollments)
                {
                    try
                    {
                        // Get user details (display ID, name)
                        var user = await _studentService.GetStudentBySupabaseIdAsync(enrollment.StudentId);
                        
                        if (user != null)
                        {
                            var fullName = $"{user.FirstName} {user.LastName}".Trim();
                            
                            enrolledStudents.Add(new CourseEnrolledStudentViewModel
                            {
                                IdNumber = user.UserDisplayId ?? "N/A",
                                FullName = fullName,
                                Status = enrollment.Status,
                                StudentId = enrollment.StudentId
                            });
                            
                            Console.WriteLine($"  - Student: {fullName} ({user.UserDisplayId}) - Status: {enrollment.Status}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  - Error fetching user details for enrollment {enrollment.StudentId}: {ex.Message}");
                    }
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    Console.WriteLine($"Filtering enrolled students by search term: '{search}'");
                    enrolledStudents = enrolledStudents
                        .Where(s => s.FullName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    s.IdNumber.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                    Console.WriteLine($"Filtered results: {enrolledStudents.Count} students");
                    ViewData["SearchTerm"] = search;
                }

                // Create the view model
                var viewModel = new ViewCourseViewModel
                {
                    Course = course,
                    EnrolledStudents = enrolledStudents
                };

                Console.WriteLine($"Passing ViewCourseViewModel to view with {enrolledStudents.Count} enrolled students");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving course: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(string id)
        {
            try
            {
                // Parse the ID to int
                if (!int.TryParse(id, out int courseId))
                    return BadRequest("Invalid course ID");

                // Fetch the course data from the service
                var course = await _courseService.GetCourseByIdAsync(courseId);

                if (course == null)
                    return NotFound("Course not found");

                // Create view model and populate with course data
                var model = new CourseEditViewModel
                {
                    CourseId = courseId,
                    Code = course.Code,
                    Name = course.Name,
                    Description = course.Description,
                    Credits = course.Credits ?? 0,
                    Level = course.Level,
                    SemesterId = course.SemesterId ?? 0,
                    MaxCapacity = course.MaxCapacity ?? 0,
                    InstructorId = course.TeacherId,
                    Status = course.Status
                };

                // Populate instructor dropdown
                var instructors = await _courseService.GetActiveInstructorsAsync();
                model.Instructors = instructors
                    .Select(i => new InstructorOption { UserTypeId = i.UserTypeId, FullName = i.FullName })
                    .ToList();

                // Populate semester dropdown
                var semesters = await _courseService.GetAllSemestersAsync();
                model.Semesters = semesters
                    .Select(s => new SemesterOption { Id = s.Id, SemesterName = s.SemesterName })
                    .ToList();

                // Populate level dropdown
                model.Levels = new List<LevelOption>
                {
                    new LevelOption { Value = "1st Year", Label = "1st Year" },
                    new LevelOption { Value = "2nd Year", Label = "2nd Year" },
                    new LevelOption { Value = "3rd Year", Label = "3rd Year" },
                    new LevelOption { Value = "4th Year", Label = "4th Year" }
                };

                Console.WriteLine($"=== AdminController.EditCourse (GET) ===");
                Console.WriteLine($"Loading course {courseId} for editing: {course.Name}");

                // Return as partial view for modal
                return PartialView("_EditCourseModal", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EditCourse form: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest("Error loading course edit form");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(CourseEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Collect all validation errors
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    var errorMessage = string.Join(", ", errors);
                    
                    Console.WriteLine($"Validation failed: {errorMessage}");
                    return Json(new { success = false, message = $"Validation error: {errorMessage}" });
                }

                Console.WriteLine($"=== AdminController.EditCourse (POST) ===");
                Console.WriteLine($"Updating course {model.CourseId}: {model.Name}");

                // Call service to update the course
                var (success, message) = await _courseService.UpdateCourseAsync(
                    courseId: model.CourseId,
                    name: model.Name,
                    description: model.Description,
                    credits: model.Credits,
                    level: model.Level,
                    semesterId: model.SemesterId,
                    maxCapacity: model.MaxCapacity,
                    instructorId: model.InstructorId,
                    status: model.Status
                );

                if (!success)
                {
                    Console.WriteLine($"Course update failed: {message}");
                    return Json(new { success = false, message = message });
                }

                Console.WriteLine($"Course updated successfully: {model.Name}");
                return Json(new { success = true, message = "Course updated successfully", courseId = model.CourseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating course: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error updating course: {ex.Message}" });
            }
        }

        [HttpGet]

        public IActionResult Teachers()
        {
            return RedirectToAction("Index", "Teacher");
        }

        [HttpGet]
        public IActionResult NotificationCount()
        {
            var count = 0; // sync with model above; replace with real count when available
            return Json(new { count });
        }

        /// <summary>
        /// Gets the admin database ID from the Supabase user ID
        /// </summary>
        private async Task<string> GetAdminIdFromSupabaseIdAsync(string supabaseUserId)
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
                Console.WriteLine($"Error getting admin ID from Supabase ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Form post to search available students for a course
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetAvailableStudentsForCourse(int courseId, string search)
        {
            try
            {
                Console.WriteLine($"=== AdminController.GetAvailableStudentsForCourse ===");
                Console.WriteLine($"Course ID: {courseId}, Search: '{search ?? ""}'");

                var availableStudents = await _courseService.GetAvailableStudentsForCourseAsync(courseId, search ?? "");

                Console.WriteLine($"Service returned {availableStudents?.Count ?? 0} raw students from database");

                var result = availableStudents
                    .Select(s => new
                    {
                        StudentId = s.UserTypeId,
                        IdNumber = s.UserDisplayId ?? "N/A",
                        FullName = $"{s.FirstName} {s.LastName}".Trim()
                    })
                    .ToList();

                Console.WriteLine($"Processed into {result.Count} available students for display");
                if (result.Count > 0)
                {
                    Console.WriteLine("Available students:");
                    foreach (var student in result)
                    {
                        Console.WriteLine($"  - ID: {student.IdNumber}, Name: {student.FullName}, StudentId: {student.StudentId}");
                    }
                }
                else
                {
                    Console.WriteLine("No available students found matching the search criteria");
                }
                
                // Fetch course and enrolled students to rebuild the view
                var course = await _courseService.GetCourseByIdAsync(courseId);
                var enrollments = await _courseService.GetCourseEnrollmentsByCourseIdAsync(courseId);
                
                var enrolledStudents = new List<CourseEnrolledStudentViewModel>();
                foreach (var enrollment in enrollments)
                {
                    var user = await _studentService.GetStudentBySupabaseIdAsync(enrollment.StudentId);
                    if (user != null)
                    {
                        enrolledStudents.Add(new CourseEnrolledStudentViewModel
                        {
                            IdNumber = user.UserDisplayId ?? "N/A",
                            FullName = $"{user.FirstName} {user.LastName}".Trim(),
                            Status = enrollment.Status,
                            StudentId = enrollment.StudentId
                        });
                    }
                }

                var viewModel = new ViewCourseViewModel
                {
                    Course = course,
                    EnrolledStudents = enrolledStudents
                };

                ViewData["AvailableStudents"] = result;
                return View("ViewCourse", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available students: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error searching students: {ex.Message}";
                return RedirectToAction("ViewCourse", new { id = courseId });
            }
        }

        /// <summary>
        /// Form post to enroll a student in a course
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EnrollStudentInCourse(int courseId, string studentId)
        {
            try
            {
                Console.WriteLine($"=== AdminController.EnrollStudentInCourse ===");
                Console.WriteLine($"Course ID: {courseId}, Student ID: {studentId}");

                var (success, message) = await _courseService.EnrollStudentInCourseAsync(courseId, studentId);

                Console.WriteLine($"Service enrollment result - Success: {success}, Message: '{message}'");

                if (success)
                {
                    Console.WriteLine($"Student enrolled successfully - Course: {courseId}, Student: {studentId}");
                    TempData["Success"] = message;
                }
                else
                {
                    Console.WriteLine($"Enrollment failed - Course: {courseId}, Student: {studentId}, Reason: {message}");
                    TempData["Error"] = message;
                }

                return RedirectToAction("ViewCourse", new { id = courseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enrolling student: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("ViewCourse", new { id = courseId });
            }
        }
    }
}

