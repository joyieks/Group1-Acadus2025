using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;
using ASI.Basecode.WebApp.Models;
using System.Collections.Generic;
using System.Security.Claims;
using ASI.Basecode.Data.Models;
using System;
using System.Linq;


namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IAdminService _adminService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public AdminController(IStudentService studentService, ITeacherService teacherService, ISupabaseAuthService supabaseAuthService, IAdminService adminService, ICourseService courseService, IUserService userService)
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

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View(new StudentCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Map ViewModel to DTO
                var studentDto = new StudentCreateDto
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
                    YearLevel = model.YearLevel,
                    ProgramId = model.ProgramId,
                    DepartmentId = model.DepartmentId,
                    EmergencyContactFirstName = model.EmergencyContactFirstName,
                    EmergencyContactMiddleName = model.EmergencyContactMiddleName,
                    EmergencyContactLastName = model.EmergencyContactLastName,
                    EmergencyContactSuffix = model.EmergencyContactSuffix,
                    EmergencyContactNumber = model.EmergencyContactNumber,
                    EmergencyContactRelationship = model.EmergencyContactRelationship
                };

                var success = await _userService.CreateStudentAsync(studentDto);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Student {model.FirstName} {model.LastName} has been successfully created!";
                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create student. Please try again.");
                    return View(model);
                }
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating student: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View(new TeacherCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(TeacherCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                    return View(model);
                }
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating teacher: {ex.Message}");
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

        [HttpGet]
        public async Task<IActionResult> AddCourse()
        {
            try
            {
                var model = new CourseCreateViewModel();
                
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
                    new LevelOption { Value = "Undergraduate", Label = "Undergraduate" },
                    new LevelOption { Value = "Graduate", Label = "Graduate" },
                    new LevelOption { Value = "Doctorate", Label = "Doctorate" }
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
                        new LevelOption { Value = "Undergraduate", Label = "Undergraduate" },
                        new LevelOption { Value = "Graduate", Label = "Graduate" },
                        new LevelOption { Value = "Doctorate", Label = "Doctorate" }
                    };

                    return View(model);
                }

                Console.WriteLine($"=== AdminController.AddCourse (POST) ===");
                Console.WriteLine($"Creating course: {model.Name} ({model.Code})");

                var (success, message, courseId) = await _courseService.CreateCourseAsync(
                    code: model.Code,
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
                        new LevelOption { Value = "Undergraduate", Label = "Undergraduate" },
                        new LevelOption { Value = "Graduate", Label = "Graduate" },
                        new LevelOption { Value = "Doctorate", Label = "Doctorate" }
                    };

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating course: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"Error creating course: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ViewCourse(string id)
        {
            // TODO: Load course data by id
            return View();
        }

        [HttpGet]
        public IActionResult EditCourse(string id)
        {
            // TODO: Load course data by id
            return View();
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
    }
}

