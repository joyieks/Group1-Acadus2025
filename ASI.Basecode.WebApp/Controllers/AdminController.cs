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
        public async Task<IActionResult> AddStudent()
        {
            // ✅ No need to load programs and departments - they're hardcoded in the view
            Console.WriteLine("Loading Add Student form with hardcoded programs and department");
            
            return View(new StudentCreateViewModel());
        }

        // POST: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ✅ No need to reload dropdowns - they're hardcoded
                return View(model);
            }

            try
            {
                // Map ViewModel to DTO for service layer
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
        Console.WriteLine($"Error creating student: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        ModelState.AddModelError(string.Empty, $"Error creating student: {ex.Message}");
        return View(model);
 }
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

  /// <summary>
        /// ViewModel for academic performance items
        /// </summary>
        public class AcademicPerformanceItem
        {
            public string CourseCode { get; set; }
   public string CourseTitle { get; set; }
    public double OverallPercentage { get; set; }
   }
    }
}

