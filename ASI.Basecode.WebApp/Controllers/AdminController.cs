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
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static ASI.Basecode.Data.Models.CourseGradebookViewModel;


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
        private readonly ITeacherCourseService _teacherCourseService;

        public AdminController(IStudentService studentService, ITeacherService teacherService, ISupabaseAuthService supabaseAuthService, IAdminService adminService, ICourseService courseService, IUserService userService, IAuditLogService auditLogService, ITeacherCourseService teacherCourseService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _supabaseAuthService = supabaseAuthService;
            _adminService = adminService;
            _courseService = courseService;
            _userService = userService;
            _auditLogService = auditLogService;
            _teacherCourseService = teacherCourseService;
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

                // ⚡ OPTIMIZED: Resolve roles for ALL users in parallel (N+1 fix)
                var uniqueUsers = allUsers
                    .GroupBy(u => u.UserTypeId)
                    .Select(g => g.First())
                    .ToList();
                
                Console.WriteLine($"⏱️ Starting parallel role resolution for {uniqueUsers.Count} users...");
                var rolesTasks = uniqueUsers.Select(async u =>
                {
                    var rolesForUser = await _userService.GetUserRolesAsync(u.UserTypeId);
                    return new UserWithRoleViewModel
                    {
                        User = u,
                        Roles = rolesForUser
                    };
                }).ToList();

                // Wait for all role queries to complete in parallel instead of sequentially
                var allUsersWithRoles = (await Task.WhenAll(rolesTasks)).ToList();

                Console.WriteLine($"✅ Resolved roles for {allUsersWithRoles.Count} users in parallel");

                // ⚡ OPTIMIZED: Filter by role ONCE and store both User and UserWithRole
                var studentsWithRoles = allUsersWithRoles
                    .Where(entry => entry.Roles.Any(r => r.RoleName != null && 
                           (r.RoleName.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Students", StringComparison.OrdinalIgnoreCase))))
                    .GroupBy(entry => entry.User.UserTypeId)  // Deduplicate by UserTypeId
                    .Select(g => g.First())     // Take first occurrence
                    .ToList();

                var instructorsWithRoles = allUsersWithRoles
                    .Where(entry => entry.Roles.Any(r => r.RoleName != null && 
                           (r.RoleName.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Instructor", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Teachers", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Instructors", StringComparison.OrdinalIgnoreCase))))
                    .GroupBy(entry => entry.User.UserTypeId)  // Deduplicate by UserTypeId
                    .Select(g => g.First())     // Take first occurrence
                    .ToList();

                // Extract just the users for the old lists (for backward compatibility)
                var students = studentsWithRoles.Select(entry => entry.User).ToList();
                var instructors = instructorsWithRoles.Select(entry => entry.User).ToList();

                Console.WriteLine($"📊 Filtered by role - Students: {students.Count}, Instructors: {instructors.Count}");

                // Determine which list to display based on tab
                List<UserWithRoleViewModel> displayedWithRoles;

                switch (tab)
                {
                    case "students":
                        displayedWithRoles = studentsWithRoles;
                        Console.WriteLine($"🎓 Displaying {displayedWithRoles.Count} students");
                        break;
                    case "instructors":
                        displayedWithRoles = instructorsWithRoles;
                        Console.WriteLine($"👨‍🏫 Displaying {displayedWithRoles.Count} instructors");
                        break;
                    default: // "all"
                        displayedWithRoles = allUsersWithRoles;
                        Console.WriteLine($"📋 Displaying {displayedWithRoles.Count} all users");
                        break;
                }

                var viewModel = new UsersTableViewModel
                {
                    AllUsers = allUsers,
                    Students = students,
                    Instructors = instructors,
                    DisplayedUsersWithRoles = displayedWithRoles,
                    SearchTerm = search,
                    ActiveTab = tab,
                    ActiveStatus = status,
                    TotalStudents = totalStudents,
                    TotalInstructors = totalInstructors
                };

                Console.WriteLine($"✅ ViewModel created - All: {allUsers.Count}, Students: {students.Count}, Instructors: {instructors.Count}, Displayed: {displayedWithRoles.Count}");
                
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
            return View(new StudentCreateViewModel());
        }

        // POST: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Map ViewModel to DTO
                var dto = new StudentCreateDto
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

                var success = await _userService.CreateStudentAsync(dto);
       
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

            return View(new StudentCreateViewModel());
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

                // Load academic performance for students
                if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var academicPerformance = await GetStudentAcademicPerformanceAsync(user.UserTypeId);
                    ViewBag.AcademicPerformance = academicPerformance;
                    Console.WriteLine($"Loaded {academicPerformance.Count} courses for student academic performance");
                }

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
        public async Task<IActionResult> Profile()
        {
            try
            {
                var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (string.IsNullOrWhiteSpace(supabaseUserId))
                {
                    ViewBag.NoDataMessage = "Session expired. Please log in again.";
                    return View("~/Views/Shared/Profile.cshtml", new StudentProfileViewModel());
                }

                var model = new StudentProfileViewModel();
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Load user info
                var user = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == supabaseUserId)
                    .Get();

                var userData = user?.Models?.FirstOrDefault();
                if (userData != null)
                {
                    model.FirstName = userData.FirstName;
                    model.MiddleName = userData.MiddleName;
                    model.LastName = userData.LastName;
                    model.Suffix = userData.Suffix;
                    model.PhoneNumber = userData.ContactNumber;
                    model.StudentId = userData.UserDisplayId ?? "N/A";
                    model.FullName = string.Join(" ", new[] { userData.FirstName, userData.MiddleName, userData.LastName, userData.Suffix }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    model.EmailAddress = userData.Email;
                    model.Status = userData.IsActive ?? false ? "Active" : "Inactive";
                }

                // Profile image
                model.ProfileImageUrl = await _supabaseAuthService.GetUserProfileImageUrlAsync(supabaseUserId);
                if (TempData["UploadedProfileUrl"] is string uploadedUrl && !string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    model.ProfileImageUrl = uploadedUrl;
                }

                // Password last updated (default to now if not available)
                model.PasswordLastUpdated = DateTime.Now.AddMonths(-1);

                return View("~/Views/Shared/Profile.cshtml", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading admin profile: {ex.Message}");
                ViewBag.NoDataMessage = "Error loading profile data. Please try again.";
                return View("~/Views/Shared/Profile.cshtml", new StudentProfileViewModel());
            }
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
                        Console.WriteLine($"  Processing enrollment: StudentId={enrollment.StudentId}, Status={enrollment.Status}, CourseId={enrollment.CourseId}");
                        
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
                            
                            Console.WriteLine($"  ✓ Added student: {fullName} ({user.UserDisplayId}) - Status: {enrollment.Status}");
                        }
                        else
                        {
                            Console.WriteLine($"  ✗ Student not found for enrollment StudentId={enrollment.StudentId}");
                            // Still add the enrollment with limited info
                            enrolledStudents.Add(new CourseEnrolledStudentViewModel
                            {
                                IdNumber = "N/A",
                                FullName = $"Student ID: {enrollment.StudentId}",
                                Status = enrollment.Status,
                                StudentId = enrollment.StudentId
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Error fetching user details for enrollment {enrollment.StudentId}: {ex.Message}");
                        Console.WriteLine($"    Stack Trace: {ex.StackTrace}");
                        // Still add the enrollment with limited info
                        enrolledStudents.Add(new CourseEnrolledStudentViewModel
                        {
                            IdNumber = "N/A",
                            FullName = $"Error loading: {enrollment.StudentId}",
                            Status = enrollment.Status,
                            StudentId = enrollment.StudentId
                        });
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

                // Fetch instructor name if instructor ID exists
                string instructorName = "N/A";
                if (!string.IsNullOrWhiteSpace(course.TeacherId))
                {
                    try
                    {
                        var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                        var instructorQuery = await client.From<SupabaseUserNew>()
                            .Where(x => x.UserTypeId == course.TeacherId)
                            .Get();
                        var instructor = instructorQuery?.Models?.FirstOrDefault();
                        
                        if (instructor != null)
                        {
                            var nameParts = new[] { instructor.FirstName, instructor.MiddleName, instructor.LastName, instructor.Suffix }
                                .Where(s => !string.IsNullOrWhiteSpace(s));
                            instructorName = string.Join(" ", nameParts);
                            if (string.IsNullOrWhiteSpace(instructorName))
                            {
                                instructorName = instructor.UserDisplayId ?? "N/A";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching instructor name: {ex.Message}");
                        instructorName = course.TeacherId; // Fallback to ID if name fetch fails
                    }
                }

                // Create the view model
                var viewModel = new ViewCourseViewModel
                {
                    Course = course,
                    EnrolledStudents = enrolledStudents,
                    InstructorName = instructorName
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
                    TempData["ErrorMessage"] = $"Validation error: {errorMessage}";
                    return RedirectToAction("ViewCourse", new { id = model.CourseId });
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
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction("ViewCourse", new { id = model.CourseId });
                }

                Console.WriteLine($"Course updated successfully: {model.Name}");
                
                // Always redirect to ViewCourse page with success message
                TempData["SuccessMessage"] = "Course updated successfully";
                return RedirectToAction("ViewCourse", new { id = model.CourseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating course: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error updating course: {ex.Message}";
                return RedirectToAction("ViewCourse", new { id = model.CourseId });
            }
        }

        [HttpGet]

        public IActionResult Teachers()
        {
            return RedirectToAction("Index", "Teacher");
        }

        [HttpPost]
        public async Task<IActionResult> SetUserStatus([FromBody] JsonElement request)
        {
            try
            {
                Console.WriteLine($"╔══════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║         SetUserStatus - Admin Action                    ║");
                Console.WriteLine($"╚══════════════════════════════════════════════════════════╝");
                
                // Extract parameters from the JsonElement request
                string id = null;
                bool isActive = false;
                
                if (request.TryGetProperty("id", out var idProperty))
                {
                    id = idProperty.GetString();
                }
                
                if (request.TryGetProperty("isActive", out var isActiveProperty))
                {
                    isActive = isActiveProperty.GetBoolean();
                }
                
                Console.WriteLine($"� COMPARISON VALUES:");
                Console.WriteLine($"   Frontend ID (from data attribute):");
                Console.WriteLine($"      Type: {id?.GetType().Name ?? "null"}");
                Console.WriteLine($"      Value: '{id}'");
                Console.WriteLine($"      Length: {id?.Length ?? 0}");
                
                Console.WriteLine($"🔄 New Status: {(isActive ? "ACTIVATE" : "DEACTIVATE")} (IsActive={isActive})");

                if (string.IsNullOrWhiteSpace(id))
                {
                    Console.WriteLine($"❌ VALIDATION FAILED: Invalid/Empty UserTypeId: {id}");
                    return Json(new { success = false, message = "Invalid user ID" });
                }

                // ⚡ OPTIMIZED: Query only the specific user by UserTypeId (N+1 fix)
                Console.WriteLine($"📥 Querying Supabase WHERE userTypeId == '{id}'...");
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                
                var userQuery = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == id)
                    .Get();

                if (userQuery?.Models == null || !userQuery.Models.Any())
                {
                    Console.WriteLine($"❌ USER NOT FOUND in Supabase");
                    Console.WriteLine($"   Query compared: userTypeId (string) == '{id}' (string)");
                    Console.WriteLine($"   No matching records found in users table");
                    return Json(new { success = false, message = $"User not found. Please refresh the page and try again." });
                }

                var user = userQuery.Models.First();
                
                Console.WriteLine($"✅ USER FOUND in Supabase:");
                Console.WriteLine($"   Supabase Record userTypeId:");
                Console.WriteLine($"      Type: {user.UserTypeId?.GetType().Name ?? "null"}");
                Console.WriteLine($"      Value: '{user.UserTypeId}'");
                Console.WriteLine($"      Length: {user.UserTypeId?.Length ?? 0}");
                Console.WriteLine($"   Match: {(user.UserTypeId == id ? "✅ EXACT MATCH" : "❌ MISMATCH")}");
                Console.WriteLine($"   User: {user.FirstName} {user.LastName}");
                Console.WriteLine($"   Current Status: {(user.IsActive == true ? "ACTIVE" : "INACTIVE")}");
                Console.WriteLine($"   New Status: {(isActive ? "ACTIVE" : "INACTIVE")}");

                // Update user status
                user.IsActive = isActive;

                // Update in Supabase
                Console.WriteLine($"💾 Updating user in Supabase...");
                
                var updateResult = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == id)
                    .Update(user);

                if (updateResult?.Models == null || !updateResult.Models.Any())
                {
                    Console.WriteLine($"❌ UPDATE FAILED: No models returned from Supabase");
                    return Json(new { success = false, message = "Failed to update user status in database" });
                }

                Console.WriteLine($"✅ SUPABASE UPDATE SUCCESSFUL");
                Console.WriteLine($"   User: {user.FirstName} {user.LastName}");
                Console.WriteLine($"   New Status: {(isActive ? "ACTIVE" : "INACTIVE")}");

                // Log admin activity
                await LogAdminActivityAsync(
                    actionType: isActive ? "ACTIVATE_USER" : "DEACTIVATE_USER",
                    actionDescription: $"Admin {(isActive ? "activated" : "deactivated")} user {user.FirstName} {user.LastName}",
                    details: $"User TypeID: {id}, Email: {user.Email}"
                );

                Console.WriteLine($"📝 Admin activity logged");
                Console.WriteLine($"═══════════════════════════════════════════════════════════");

                return Json(new { success = true, message = $"User status updated to {(isActive ? "Active" : "Inactive")}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION IN SetUserStatus: {ex.Message}");
                Console.WriteLine($"📌 Stack Trace: {ex.StackTrace}");
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
        /// Gets available students for a course (not enrolled)
        /// </summary>
        [HttpGet("Admin/GetAvailableStudentsForCourse")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetAvailableStudentsForCourse([FromQuery] int courseId, [FromQuery] string search = "")
        {
            try
            {
                Console.WriteLine($"=== AdminController.GetAvailableStudentsForCourse START ===");
                Console.WriteLine($"Course ID: {courseId}, Search: '{search ?? ""}'");
                Console.WriteLine($"Request Path: {Request.Path}");
                Console.WriteLine($"Request QueryString: {Request.QueryString}");
                
                if (courseId <= 0)
                {
                    Console.WriteLine("ERROR: Invalid course ID");
                    return new JsonResult(new { 
                        success = false, 
                        message = "Invalid course ID" 
                    })
                    {
                        ContentType = "application/json"
                    };
                }

                Console.WriteLine($"=== AdminController.GetAvailableStudentsForCourse ===");
                Console.WriteLine($"Course ID: {courseId}, Search: '{search ?? ""}'");

                // Get enrolled student IDs
                var enrollments = await _courseService.GetCourseEnrollmentsByCourseIdAsync(courseId);
                var enrolledStudentIds = (enrollments ?? new List<EnrollmentModel>())
                    .Where(e => e != null && 
                           !string.IsNullOrWhiteSpace(e.Status) && 
                           (e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)) && 
                           e.DroppedAt == null)
                    .Select(e => e.StudentId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                Console.WriteLine($"Enrolled student IDs: {string.Join(", ", enrolledStudentIds)}");

                // Get all students
                var allStudents = await _userService.GetStudentsAsync() ?? new List<SupabaseUserNew>();
                Console.WriteLine($"Total students retrieved: {allStudents.Count}");

                if (allStudents.Count == 0)
                {
                    Console.WriteLine("WARNING: No students found in database!");
                    return new JsonResult(new { 
                        success = true, 
                        students = new List<object>(),
                        debug = new {
                            totalStudents = 0,
                            enrolledCount = enrolledStudentIds.Count,
                            message = "No students found in database. Please create students first."
                        }
                    })
                    {
                        ContentType = "application/json"
                    };
                }

                // Filter out already enrolled students
                var availableStudents = allStudents
                    .Where(s => !enrolledStudentIds.Contains(s.UserTypeId))
                    .ToList();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    availableStudents = availableStudents
                        .Where(s => 
                            (s.UserDisplayId ?? "").ToLower().Contains(searchLower) ||
                            (s.FirstName ?? "").ToLower().Contains(searchLower) ||
                            (s.LastName ?? "").ToLower().Contains(searchLower))
                        .ToList();
                }

                var result = availableStudents
                    .Select(s => new
                    {
                        studentId = s.UserTypeId,
                        idNumber = s.UserDisplayId ?? "N/A",
                        firstName = s.FirstName ?? "",
                        lastName = s.LastName ?? "",
                        status = s.IsActive == true ? "Active" : "Inactive"
                    })
                    .ToList();

                Console.WriteLine($"Available students (not enrolled): {result.Count}");
                Console.WriteLine($"=== GetAvailableStudentsForCourse END ===");

                return new JsonResult(new { 
                    success = true, 
                    students = result
                })
                {
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available students: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new JsonResult(new { 
                    success = false, 
                    message = $"Error loading students: {ex.Message}" 
                })
                {
                    ContentType = "application/json"
                };
            }
        }

        /// <summary>
        /// Form post to enroll a student in a course
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EnrollStudentInCourse(int courseId, string studentId)
        {
            try
            {
                Console.WriteLine($"=== AdminController.EnrollStudentInCourse ===");
                Console.WriteLine($"Course ID: {courseId}, Student ID: {studentId}");

                var (success, message) = await _courseService.EnrollStudentInCourseAsync(courseId, studentId);

                Console.WriteLine($"Service enrollment result - Success: {success}, Message: '{message}'");

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    if (success)
                    {
                        Console.WriteLine($"Student enrolled successfully - Course: {courseId}, Student: {studentId}");
                        return Json(new { success = true, message = message });
                    }
                    else
                    {
                        Console.WriteLine($"Enrollment failed - Course: {courseId}, Student: {studentId}, Reason: {message}");
                        return Json(new { success = false, message = message });
                    }
                }
                else
                {
                    // Regular form submission - use redirect
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enrolling student: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Error: {ex.Message}" });
                }
                else
                {
                    TempData["Error"] = $"Error: {ex.Message}";
                    return RedirectToAction("ViewCourse", new { id = courseId });
                }
            }
        }

        /// <summary>
        /// Drops a student from a course.
        /// </summary>
        /// <param name="courseId">The course ID.</param>
        /// <param name="studentId">The student ID (UUID string).</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost("Admin/DropStudent")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DropStudent([FromQuery] int courseId, [FromQuery] string studentId)
        {
            try
            {
                Console.WriteLine($"=== AdminController.DropStudent START ===");
                Console.WriteLine($"CourseId: {courseId}, StudentId: {studentId}");

                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return Json(new { success = false, message = "Student ID is required." });
                }

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Find the enrollment - get all enrollments for course, filter in memory
                Console.WriteLine($"Querying enrollments for course_id = {courseId}");
                var enrollmentResponse = await client.From<EnrollmentModel>()
                    .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, (long)courseId)
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
                // Update enrollment status to "Dropped"
                enrollment.Status = "Dropped";
                enrollment.DroppedAt = DateTime.UtcNow;
                await enrollment.Update<EnrollmentModel>();

                Console.WriteLine($"=== AdminController.DropStudent SUCCESS ===");

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
                        userRole: "Admin",
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
                catch (Exception auditEx)
                {
                    Console.WriteLine($"Error logging audit activity: {auditEx.Message}");
                }

                return Json(new { success = true, message = "Student removed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dropping student: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error removing student: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets academic performance data for a student (courses enrolled and overall percentage)
        /// </summary>
        private async Task<List<AcademicPerformanceItem>> GetStudentAcademicPerformanceAsync(string studentId)
        {
            var performanceList = new List<AcademicPerformanceItem>();

            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get active enrollments for this student
                var enrollmentsResponse = await client
                    .From<EnrollmentModel>()
                    .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                    .Get();

                var enrollments = enrollmentsResponse?.Models ?? new List<EnrollmentModel>();
                var activeEnrollments = enrollments
                    .Where(e => !string.IsNullOrEmpty(e.Status) && 
                           (e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)) &&
                           e.DroppedAt == null)
                    .ToList();

                Console.WriteLine($"Found {activeEnrollments.Count} active enrollments for student {studentId}");

                // For each enrollment, get course details and calculate percentage
                foreach (var enrollment in activeEnrollments)
                {
                    try
                    {
                        // Get course details
                        var courseQuery = await client
                            .From<CourseModel>()
                            .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, enrollment.CourseId)
                            .Get();
                        var course = courseQuery?.Models?.FirstOrDefault();

                        if (course == null)
                        {
                            Console.WriteLine($"Course {enrollment.CourseId} not found for enrollment");
                            continue;
                        }

                        // Get student grade detail for this course
                        var gradeDetail = await _teacherCourseService.GetStudentGradeDetailAsync(studentId, (int)enrollment.CourseId);

                        performanceList.Add(new AcademicPerformanceItem
                        {
                            CourseCode = course.Code ?? "N/A",
                            CourseTitle = course.Name ?? "Untitled Course",
                            OverallPercentage = gradeDetail?.Percentage ?? 0
                        });

                        Console.WriteLine($"Course: {course.Code} - Percentage: {gradeDetail?.Percentage ?? 0}%");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading grade for course {enrollment.CourseId}: {ex.Message}");
                        // Continue with other courses even if one fails
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading academic performance: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return performanceList;
        }

        /// <summary>
        /// ViewModel for academic performance items
        /// </summary>
        public class AcademicPerformanceItem
        {
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public double OverallPercentage { get; set; }
        }

        /// <summary>
        /// Gets student details (profile and grades) for a specific course
        /// </summary>
        [HttpGet]
        [Route("Admin/GetStudentDetails")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetStudentDetails([FromQuery] string studentId, [FromQuery] int courseId)
        {
            try
            {
                Console.WriteLine($"=== GetStudentDetails ===");
                Console.WriteLine($"StudentId: {studentId}, CourseId: {courseId}");

                if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
                {
                    return Json(new { success = false, message = "Invalid student ID or course ID" });
                }

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get student profile information
                var userQuery = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == studentId)
                    .Get();
                var user = userQuery?.Models?.FirstOrDefault();

                if (user == null)
                {
                    Console.WriteLine($"Student not found: {studentId}");
                    return Json(new { success = false, message = "Student not found" });
                }

                Console.WriteLine($"User found: {user.FirstName} {user.LastName}");

                // Get student profile (academic info)
                var studentProfileQuery = await client.From<Student>()
                    .Where(x => x.StudentId == studentId)
                    .Get();
                var studentProfile = studentProfileQuery?.Models?.FirstOrDefault();

                // Get student grades for this course
                StudentGradeDetail gradeDetail = null;
                try
                {
                    gradeDetail = await _teacherCourseService.GetStudentGradeDetailAsync(studentId, courseId);
                    Console.WriteLine($"Grade detail loaded: {gradeDetail?.Activities?.Count ?? 0} activities");
                }
                catch (Exception gradeEx)
                {
                    Console.WriteLine($"Warning: Error loading grades: {gradeEx.Message}");
                    // Continue with empty grades - not critical
                    gradeDetail = new StudentGradeDetail
                    {
                        StudentId = studentId,
                        StudentDisplayId = user.UserDisplayId ?? "N/A",
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Activities = new List<CourseGradebookViewModel.ActivityGradeItem>()
                    };
                }

                // Build profile data
                var profileData = new
                {
                    idNumber = user.UserDisplayId ?? "N/A",
                    firstName = user.FirstName ?? "",
                    middleName = user.MiddleName ?? "",
                    lastName = user.LastName ?? "",
                    suffix = user.Suffix ?? "",
                    fullName = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName, user.Suffix }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    email = user.Email ?? "N/A",
                    phoneNumber = user.ContactNumber ?? "N/A",
                    status = user.IsActive ?? false ? "Active" : "Inactive",
                    department = studentProfile?.DepartmentId?.ToString() ?? "N/A",
                    course = studentProfile?.ProgramId?.ToString() ?? "N/A",
                    yearLevel = studentProfile?.YearLevel?.ToString() ?? "N/A"
                };

                // Build grades data
                var activitiesList = new List<object>();
                if (gradeDetail?.Activities != null && gradeDetail.Activities.Any())
                {
                    activitiesList = gradeDetail.Activities.Select(a => new
                    {
                        activityId = a.ActivityId,
                        activityName = a.ActivityName,
                        rawScore = a.RawScore,
                        maxScore = a.MaxScore,
                        percentage = a.MaxScore > 0 ? Math.Round((double)a.RawScore / a.MaxScore * 100, 1) : 0
                    }).Cast<object>().ToList();
                }

                var gradesData = new
                {
                    totalRawScore = gradeDetail?.TotalRawScore ?? 0,
                    totalMaxScore = gradeDetail?.TotalMaxScore ?? 0,
                    percentage = gradeDetail?.Percentage ?? 0,
                    activities = activitiesList
                };

                Console.WriteLine($"Returning student details: Profile={profileData.fullName}, Grades={gradesData.activities.Count} activities");

                var result = new
                {
                    success = true,
                    profile = profileData,
                    grades = gradesData
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR getting student details: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error loading student details: {ex.Message}" });
            }
        }

        /// <summary>
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

