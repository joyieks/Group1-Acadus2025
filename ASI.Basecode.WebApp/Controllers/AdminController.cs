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
    [Authorize(Roles = "Admin")]  // ? CRITICAL: Require authentication and Admin role

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
        public async Task<IActionResult> Users(string tab = "all", string search = null, string status = "all")
        {
            try
            {
                var (totalStudents, totalInstructors, totalCourses) = await _adminService.GetDashboardStatisticsAsync();
                
                Console.WriteLine($"=== AdminController.Users ===");
                Console.WriteLine($"Tab: {tab}, Search: {search ?? "none"}, Status: {status}");

                // Fetch all users first
                List<SupabaseUserNew> allUsers = await _userService.GetAllUsersAsync();
                
                Console.WriteLine($"Fetched {allUsers.Count} total users from database");

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    allUsers = allUsers
                        .Where(u =>
                            (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                            (u.LastName != null && u.LastName.ToLower().Contains(searchLower)) ||
                            (u.Email != null && u.Email.ToLower().Contains(searchLower)))
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

                // Resolve roles for ALL users first
                var allUsersWithRoles = new List<UserWithRoleViewModel>();
                foreach (var u in allUsers)
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

                // Now filter by role for students and instructors
                var students = allUsersWithRoles
                    .Where(entry => entry.Roles.Any(r => r.RoleName != null && 
                           (r.RoleName.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Students", StringComparison.OrdinalIgnoreCase))))
                    .Select(entry => entry.User)
                    .ToList();

                var instructors = allUsersWithRoles
                    .Where(entry => entry.Roles.Any(r => r.RoleName != null && 
                           (r.RoleName.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Instructor", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Teachers", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Instructors", StringComparison.OrdinalIgnoreCase))))
                    .Select(entry => entry.User)
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
                Console.WriteLine($"=== USER IDs DEBUG ===");
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

        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            // ? Load programs and departments from database for dropdowns
     try
     {
     var programs = await _adminService.GetAllProgramsAsync();
       var departments = await _adminService.GetAllDepartmentsAsync();

      ViewBag.Programs = programs;
          ViewBag.Departments = departments;
      
       Console.WriteLine($"Loaded {programs.Count} programs and {departments.Count} departments for Add Student form");
            }
     catch (Exception ex)
    {
      Console.WriteLine($"Error loading programs/departments: {ex.Message}");
   ViewBag.Programs = new List<ASI.Basecode.Data.Models.Program>();
        ViewBag.Departments = new List<Department>();
   }

      return View(new StudentCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ? FIX: Reload dropdown data when validation fails
       try
  {
        var programs = await _adminService.GetAllProgramsAsync();
         var departments = await _adminService.GetAllDepartmentsAsync();

    ViewBag.Programs = programs;
          ViewBag.Departments = departments;
           }
  catch (Exception ex)
      {
         Console.WriteLine($"Error reloading dropdowns: {ex.Message}");
        ViewBag.Programs = new List<ASI.Basecode.Data.Models.Program>();
     ViewBag.Departments = new List<Department>();
  }

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
      
     // ? FIX: Reload dropdown data before returning view
    try
   {
      var programs = await _adminService.GetAllProgramsAsync();
 var departments = await _adminService.GetAllDepartmentsAsync();
 ViewBag.Programs = programs;
    ViewBag.Departments = departments;
   }
 catch (Exception reloadEx)
 {
Console.WriteLine($"Error reloading dropdowns: {reloadEx.Message}");
    ViewBag.Programs = new List<ASI.Basecode.Data.Models.Program>();
ViewBag.Departments = new List<Department>();
      }

 return View(model);
}
  }
            catch (System.Exception ex)
   {
  ModelState.AddModelError(string.Empty, $"Error creating student: {ex.Message}");
     
 // ? FIX: Reload dropdown data before returning view
     try
    {
     var programs = await _adminService.GetAllProgramsAsync();
      var departments = await _adminService.GetAllDepartmentsAsync();
       ViewBag.Programs = programs;
    ViewBag.Departments = departments;
     }
 catch (Exception reloadEx)
     {
   Console.WriteLine($"Error reloading dropdowns: {reloadEx.Message}");
        ViewBag.Programs = new List<ASI.Basecode.Data.Models.Program>();
  ViewBag.Departments = new List<Department>();
    }

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

                // TODO: Update address and emergency contact if needed

                TempData["SuccessMessage"] = $"User {model.FirstName} {model.LastName} has been updated successfully!";
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
    }
}

