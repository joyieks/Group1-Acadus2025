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
        private readonly IAuditLogService _auditLogService;

        public AdminController(IStudentService studentService, ITeacherService teacherService, ISupabaseAuthService supabaseAuthService, IAdminService adminService, ICourseService courseService, IUserService userService, IAuditLogService auditLogService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _supabaseAuthService = supabaseAuthService;
            _adminService = adminService;
            _courseService = courseService;
            _userService = userService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        { 
            Console.WriteLine($"=== AdminController.Dashboard ===");
            
            // Run both queries in parallel for better performance
            var statisticsTask = _adminService.GetDashboardStatisticsAsync();
            var activitiesTask = _auditLogService.GetAllRecentActivitiesAsync(limit: 5); // Reduced to 5
            
            await Task.WhenAll(statisticsTask, activitiesTask);
            
            var (totalStudents, totalInstructors, totalCourses) = statisticsTask.Result;
            var recentActivities = activitiesTask.Result;
            
            Console.WriteLine($"Received from service - Students: {totalStudents}, Instructors: {totalInstructors}, Courses: {totalCourses}");
            Console.WriteLine($"Retrieved {recentActivities.Count} recent activities");
            
            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses,
                RecentActivities = recentActivities
            };
            
            Console.WriteLine($"ViewModel created - Students: {viewModel.TotalStudents}, Instructors: {viewModel.TotalInstructors}, Courses: {viewModel.TotalCourses}, Recent Activities: {viewModel.RecentActivities.Count}");
            Console.WriteLine($"=== End Dashboard ===");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users(string tab = "all", string search = null, string status = "all")
        {
            try
            {
                var (totalStudents, totalInstructors, totalCourses) = await _adminService.GetDashboardStatisticsAsync();

                Console.WriteLine($"=== AdminController.Users ===");
                Console.WriteLine($"Tab: {tab}, Search: {search ?? "none"}, Status: {status}");

                // Fetch all users first and deduplicate by UserTypeId
                List<SupabaseUserNew> allUsersRaw = await _userService.GetAllUsersAsync();
                List<SupabaseUserNew> allUsers = allUsersRaw
                    .GroupBy(u => u.UserTypeId)
                    .Select(g => g.First())
                    .ToList();
                
                Console.WriteLine($"Fetched {allUsersRaw.Count} total users from database, {allUsers.Count} unique users after deduplication");

                // Apply search filter if provided (search by name and ID number)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    allUsers = allUsers
                        .Where(u =>
                            (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                            (u.LastName != null && u.LastName.ToLower().Contains(searchLower)) ||
                            (u.UserDisplayId != null && u.UserDisplayId.ToLower().Contains(searchLower)))
                        .ToList();
                    Console.WriteLine($"After search filter: {allUsers.Count} users");
                }

                // Apply status filter (all, active, inactive)
                if (status == "active")
                {
                    allUsers = allUsers.Where(u => u.IsActive == true).ToList();
                }
                else if (status == "inactive")
                {
                    allUsers = allUsers.Where(u => u.IsActive == false).ToList();
                }
                Console.WriteLine($"After status filter ({status}): {allUsers.Count} users");

                // Resolve roles for ALL users first (deduplicate users first to avoid processing same user multiple times)
                var uniqueUsers = allUsers
                    .GroupBy(u => u.UserTypeId)
                    .Select(g => g.First())
                    .ToList();
                
                var allUsersWithRoles = new List<UserWithRoleViewModel>();
                foreach (var u in uniqueUsers)
                {
                    // FIX: Use UserTypeId (Supabase Auth UUID) not Id (database integer)
                    var rolesForUser = await _userService.GetUserRolesAsync(u.UserTypeId);
                    allUsersWithRoles.Add(new UserWithRoleViewModel
                    {
                        User = u,
                        Roles = rolesForUser
                    });
                }

                Console.WriteLine($"Resolved roles for {allUsersWithRoles.Count} users");

                // Now filter by role for students and instructors (with deduplication)
                var students = allUsersWithRoles
                    .Where(entry => entry.Roles.Any(r => r.RoleName != null && 
                           (r.RoleName.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Students", StringComparison.OrdinalIgnoreCase))))
                    .Select(entry => entry.User)
                    .GroupBy(u => u.UserTypeId)  // Group by UserTypeId to remove duplicates
                    .Select(g => g.First())     // Take first occurrence of each user
                    .ToList();

                var instructors = allUsersWithRoles
                    .Where(entry => entry.Roles.Any(r => r.RoleName != null && 
                           (r.RoleName.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Instructor", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Teachers", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Instructors", StringComparison.OrdinalIgnoreCase))))
                    .Select(entry => entry.User)
                    .GroupBy(u => u.UserTypeId)  // Group by UserTypeId to remove duplicates
                    .Select(g => g.First())       // Take first occurrence of each user
                    .ToList();

                Console.WriteLine($"Filtered by role - Students: {students.Count}, Instructors: {instructors.Count}");

                // Determine which list to display based on tab
                List<SupabaseUserNew> displayedUsers;
                List<UserWithRoleViewModel> displayedWithRoles;

                switch (tab)
                {
                    case "students":
                        displayedUsers = students;
                        displayedWithRoles = allUsersWithRoles
                            .Where(entry => students.Any(s => s.UserTypeId == entry.User.UserTypeId))
                            .ToList();
                        break;
                    case "instructors":
                        displayedUsers = instructors;
                        displayedWithRoles = allUsersWithRoles
                            .Where(entry => instructors.Any(i => i.UserTypeId == entry.User.UserTypeId))
                            .ToList();
                        break;
                    default: // "all"
                        displayedUsers = allUsers;
                        displayedWithRoles = allUsersWithRoles;
                        break;
                }

                Console.WriteLine($"Displaying {displayedWithRoles.Count} users for tab '{tab}'");

                var viewModel = new UsersTableViewModel
                {
                    AllUsers = allUsers,
                    Students = students,
                    Instructors = instructors,
                    DisplayedUsers = displayedUsers,
                    DisplayedUsersWithRoles = displayedWithRoles,
                    SearchTerm = search,
                    ActiveTab = tab,
                    ActiveStatus = status,
                    TotalStudents = totalStudents,
                    TotalInstructors = totalInstructors
                };

                Console.WriteLine($"ViewModel created - All: {allUsers.Count}, Students: {students.Count}, Instructors: {instructors.Count}, Displayed: {displayedUsers.Count}");
                
                // ? ADDED: Log user IDs for debugging
                Console.WriteLine($"=== USER IDS DEBUG ===");
                foreach (var entry in displayedWithRoles.Take(3))
                {
                    Console.WriteLine($"User: {entry.User.FirstName} {entry.User.LastName}");
                    Console.WriteLine($"  - Database ID: {entry.User.Id}");
                    Console.WriteLine($"  - UserTypeId (UUID): {entry.User.UserTypeId}");
                    Console.WriteLine($"  - IsActive: {entry.User.IsActive}");
                    Console.WriteLine($"  - Roles: {string.Join(", ", entry.Roles.Select(r => r.RoleName))}");
                }
                
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
       
                if (success)
                {
                    // Log admin activity
                    await LogAdminActivityAsync(
                        actionType: "CREATE_STUDENT",
                        actionDescription: $"Admin created student {model.FirstName} {model.LastName}",
                        details: $"Email: {model.Email}, Program: {model.ProgramId}, Department: {model.DepartmentId}"
                    );

                    TempData["UserSuccessMessage"] = $"Student {model.FirstName} {model.LastName} has been successfully created!";
                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create student. Please try again.");
                    return View(model);
                }
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
                    // Log admin activity
                    await LogAdminActivityAsync(
                        actionType: "CREATE_TEACHER",
                        actionDescription: $"Admin created teacher {model.FirstName} {model.LastName}",
                        details: $"Email: {model.Email}, Department: {model.DepartmentId}"
                    );

                    TempData["UserSuccessMessage"] = $"Teacher {model.FirstName} {model.LastName} has been successfully created!";
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
        public async Task<IActionResult> ViewUser(int id)
        {
            try
            {
                Console.WriteLine($"=== ViewUser GET: Loading user with ID={id} ===");

                // Get all users
                var allUsers = await _userService.GetAllUsersAsync();
                var user = allUsers.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Users");
                }

                // Get user roles to determine if Student or Teacher
                var roles = await _userService.GetUserRolesAsync(user.UserTypeId);
                var role = roles.FirstOrDefault()?.RoleName ?? "Unknown";

                Console.WriteLine($"Found user: {user.FirstName} {user.LastName}, Role: {role}");

                // Store role and other data in ViewBag for the view
                ViewBag.Role = role;
                ViewBag.Department = "CCS"; // TODO: Load from actual department table

                Console.WriteLine($"ViewUser page loaded for user ID {id}");
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user for view: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Error loading user data.";
                return RedirectToAction("Users");
            }
        }

        [HttpGet]
        public IActionResult ViewTeacher(string id)
        {
            // TODO: Load teacher data by id
            return View();
        }

        /// <summary>
        /// GET: Loads user data for editing
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                Console.WriteLine($"\n=== EditUser GET: Loading user with ID={id} ===");

                if (id <= 0)
                {
                    Console.WriteLine($"? Invalid ID provided: {id}");
                    TempData["ErrorMessage"] = "Invalid user ID.";
                    return RedirectToAction("Users");
                }

                // Get all users
                Console.WriteLine($"Fetching all users from database...");
                var allUsers = await _userService.GetAllUsersAsync();
                Console.WriteLine($"Retrieved {allUsers.Count} total users");

                // Log first few user IDs for debugging
                if (allUsers.Count > 0)
                {
                    Console.WriteLine($"Sample user IDs in database:");
                    foreach (var u in allUsers.Take(5))
                    {
                        Console.WriteLine($"  - ID: {u.Id}, Name: {u.FirstName} {u.LastName}, Email: {u.Email}");
                    }
                }

                Console.WriteLine($"Searching for user with ID={id}...");
                var user = allUsers.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    Console.WriteLine($"? User not found with ID: {id}");
                    Console.WriteLine($"Available user IDs: {string.Join(", ", allUsers.Select(u => u.Id).Take(20))}");
                    TempData["ErrorMessage"] = $"User with ID {id} not found.";
                    return RedirectToAction("Users");
                }

                Console.WriteLine($"? Found user: {user.FirstName} {user.LastName} (Email: {user.Email})");

                // Get user roles to determine if Student or Teacher
                Console.WriteLine($"Fetching roles for user with UserTypeId: {user.UserTypeId}");
                var roles = await _userService.GetUserRolesAsync(user.UserTypeId);
                var role = roles.FirstOrDefault()?.RoleName ?? "Unknown";

                Console.WriteLine($"User role: {role}");

                // Store role and other data in ViewBag for the view
                ViewBag.Role = role;
                ViewBag.Department = "CCS"; // TODO: Load from actual department table

                Console.WriteLine($"? EditUser view loaded successfully for user ID {id}");
                Console.WriteLine($"=== End EditUser GET ===\n");
                
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR in EditUser GET:");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = "Error loading user data for editing.";
                return RedirectToAction("Users");
            }
        }

        /// <summary>
        /// POST: Saves edited user data
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, SupabaseUserNew model)
        {
            try
            {
                Console.WriteLine($"=== EditUser POST: Saving user with ID={id} ===");

                // Validate ID match
                if (id != model.Id)
                {
                    Console.WriteLine($"ID mismatch: URL id={id}, Model id={model.Id}");
                    ModelState.AddModelError(string.Empty, "User ID mismatch.");
                    return View(model);
                }

                // Get the existing user
                var allUsers = await _userService.GetAllUsersAsync();
                var existingUser = allUsers.FirstOrDefault(u => u.Id == id);

                if (existingUser == null)
                {
                    Console.WriteLine($"User with ID {id} not found");
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Users");
                }

                Console.WriteLine($"Updating user: {existingUser.FirstName} {existingUser.LastName}");

                // Update only the editable fields from the form
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.MiddleName = model.MiddleName;
                existingUser.Suffix = model.Suffix;
                existingUser.Email = model.Email;
                existingUser.ContactNumber = model.ContactNumber;
                existingUser.IsActive = model.IsActive;

                // Save to database
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                
                var updateResult = await client.From<SupabaseUserNew>()
                    .Where(x => x.Id == id)
                    .Update(existingUser);

                if (updateResult?.Models == null || !updateResult.Models.Any())
                {
                    Console.WriteLine("Update failed - no models returned");
                    ModelState.AddModelError(string.Empty, "Failed to update user.");
                    return View(model);
                }

                Console.WriteLine($"? User {existingUser.FirstName} {existingUser.LastName} updated successfully");

                // Log admin activity
                await LogAdminActivityAsync(
                    actionType: "UPDATE_USER",
                    actionDescription: $"Admin updated user {model.FirstName} {model.LastName}",
                    details: $"User ID: {id}, Email: {model.Email}"
                );

                // TODO: Update address and emergency contact if needed

                TempData["UserSuccessMessage"] = $"User {model.FirstName} {model.LastName} has been updated successfully!";
                return RedirectToAction("ViewUser", new { id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"Error updating user: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// API endpoint to get all recent activities for admin (with optional role filter)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRecentActivitiesApi(string role = "all")
        {
            try
            {
                List<AuditLogModel> activities;
                
                // Filter by role if specified
                if (role == "all" || string.IsNullOrWhiteSpace(role))
                {
                    activities = await _auditLogService.GetAllRecentActivitiesAsync(limit: 100);
                }
                else
                {
                    // Capitalize first letter for consistent role names
                    var roleFilter = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    activities = await _auditLogService.GetRecentActivitiesByRoleAsync(roleFilter, limit: 100);
                }
                
                var activitiesData = activities.Select(a => new
                {
                    actionDescription = a.ActionDescription,
                    userName = a.UserName,
                    createdAt = a.CreatedAt.Kind == DateTimeKind.Utc ? a.CreatedAt.ToLocalTime() : a.CreatedAt,
                    formattedDate = (a.CreatedAt.Kind == DateTimeKind.Utc ? a.CreatedAt.ToLocalTime() : a.CreatedAt).ToString("MMMM dd, yyyy, hh:mm tt")
                }).ToList();

                return Json(new { success = true, activities = activitiesData, role = role });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all recent activities: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> RecentActivity(string role = "all")
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                Console.WriteLine($"=== RecentActivity: Loading activities for role={role} ===");
                Console.WriteLine($"[{stopwatch.ElapsedMilliseconds}ms] Starting query...");

                List<AuditLogModel> activities;
                
                // Filter by role if specified
                if (role == "all" || string.IsNullOrWhiteSpace(role))
                {
                    activities = await _auditLogService.GetAllRecentActivitiesAsync(limit: 20);
                }
                else
                {
                    // Capitalize first letter for consistent role names
                    var roleFilter = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    activities = await _auditLogService.GetRecentActivitiesByRoleAsync(roleFilter, limit: 20);
                }
                
                Console.WriteLine($"[{stopwatch.ElapsedMilliseconds}ms] Query completed - Retrieved {activities.Count} activities");
                
                stopwatch.Stop();
                Console.WriteLine($"[TOTAL TIME: {stopwatch.ElapsedMilliseconds}ms]");

                // Pass the current role filter to the view
                ViewBag.CurrentRole = role;

                return View(activities);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recent activity: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return View(new List<AuditLogModel>());
            }
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
                    // Log admin activity
                    await LogAdminActivityAsync(
                        actionType: "CREATE_COURSE",
                        actionDescription: $"Admin created course {model.Name} ({model.Code})",
                        details: $"Credits: {model.Credits}, Level: {model.Level}, Max Capacity: {model.MaxCapacity}"
                    );

                    TempData["CourseSuccessMessage"] = $"Course '{model.Name}' has been created successfully!";
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

        [HttpPost]
        public async Task<IActionResult> SetUserStatus(int id, bool isActive)
        {
            try
            {
                Console.WriteLine($"=== SetUserStatus: ID={id}, IsActive={isActive} ===");

                if (id <= 0)
                {
                    Console.WriteLine($"? Invalid ID: {id}");
                    return Json(new { success = false, message = "Invalid user ID" });
                }

                // Get all users from database
                Console.WriteLine($"Calling GetAllUsersAsync()...");
                var allUsers = await _userService.GetAllUsersAsync();
                Console.WriteLine($"Retrieved {allUsers.Count} users from GetAllUsersAsync()");
                
                // Log first few users to verify data
                if (allUsers.Count > 0)
                {
                    Console.WriteLine($"First 5 users in database:");
                    foreach (var u in allUsers.Take(5))
                    {
                        Console.WriteLine($"  - ID: {u.Id}, Name: {u.FirstName} {u.LastName}, Email: {u.Email}, IsActive: {u.IsActive}");
                    }
                }
                
                Console.WriteLine($"Searching for user with ID={id}...");
                var user = allUsers.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    Console.WriteLine($"? User not found with ID: {id}");
                    Console.WriteLine($"? Available user IDs: {string.Join(", ", allUsers.Select(u => u.Id).Take(10))}");
                    return Json(new { success = false, message = $"User not found with ID {id}. Please refresh the page and try again." });
                }

                Console.WriteLine($"? Found user: {user.FirstName} {user.LastName} (ID: {user.Id})");
                Console.WriteLine($"Current status: {user.IsActive}, New status: {isActive}");

                // Update user status
                user.IsActive = isActive;

                // Update in Supabase
                Console.WriteLine($"Updating user in Supabase...");
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                
                var updateResult = await client.From<SupabaseUserNew>()
                    .Where(x => x.Id == id)
                    .Update(user);

                if (updateResult?.Models == null || !updateResult.Models.Any())
                {
                    Console.WriteLine($"? Update failed - no models returned");
                    return Json(new { success = false, message = "Failed to update user status in database" });
                }

                Console.WriteLine($"? User {user.FirstName} {user.LastName} status updated to {(isActive ? "Active" : "Inactive")}");

                // Log admin activity
                await LogAdminActivityAsync(
                    actionType: isActive ? "ACTIVATE_USER" : "DEACTIVATE_USER",
                    actionDescription: $"Admin {(isActive ? "activated" : "deactivated")} user {user.FirstName} {user.LastName}",
                    details: $"User ID: {id}, Email: {user.Email}"
                );

                return Json(new { success = true, message = $"User status updated to {(isActive ? "Active" : "Inactive")}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error updating user status: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
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
<<<<<<< HEAD
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
=======
        /// Logs an admin activity to the audit_logs table
        /// </summary>
        /// <param name="actionType">Type of action (e.g., "CREATE_USER", "UPDATE_USER", "DELETE_USER", "CREATE_COURSE")</param>
        /// <param name="actionDescription">Human-readable description (e.g., "Admin created student John Doe")</param>
        /// <param name="details">Optional JSON string with additional data</param>
        private async Task LogAdminActivityAsync(string actionType, string actionDescription, string details = null)
        {
            try
            {
                // Get current admin user info from session/claims
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                var currentUserName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Admin";

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    Console.WriteLine("Warning: Could not log admin activity - no user ID found in session");
                    return;
                }

                await _auditLogService.LogActivityAsync(
                    userId: currentUserId,
                    userRole: "Admin",
                    userName: currentUserName,
                    actionType: actionType,
                    actionDescription: actionDescription,
                    details: details
                );

                Console.WriteLine($"Admin activity logged: {actionType} - {actionDescription}");
            }
            catch (Exception ex)
            {
                // Don't throw - audit logging should not break the main flow
                Console.WriteLine($"Error logging admin activity: {ex.Message}");
            }
        }
    }
}

