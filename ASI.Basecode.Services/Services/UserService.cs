using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IStudentService _studentService; 
        private readonly ITeacherService _teacherService; 

        public UserService(ISupabaseAuthService supabaseAuthService, IMapper mapper = null, IUserRepository repository = null, IStudentService studentService = null, ITeacherService teacherService = null)
        {
            _mapper = mapper;
            _repository = repository;
            _supabaseAuthService = supabaseAuthService;
            _studentService = studentService;
            _teacherService = teacherService; 
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId &&
                                                     x.Password == passwordKey).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        /// <summary>
        /// Retrieves all users from Supabase.
        /// </summary>
        public async Task<List<SupabaseUserNew>> GetAllUsersAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var usersQuery = await client
                    .From<SupabaseUserNew>()
                    .Get();

                var usersList = usersQuery?.Models ?? new List<SupabaseUserNew>();

                Console.WriteLine($"Retrieved {usersList.Count} users from database");
                return usersList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all users: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SupabaseUserNew>();
            }
        }

        /// <summary>
        /// Retrieves all students (roleId = 1).
        /// </summary>
        public async Task<List<SupabaseUserNew>> GetStudentsAsync()
        {
            try
            {
                var allUsers = await GetAllUsersAsync();
                var students = allUsers.Where(u => u.UserTypeId == "1" && (u.IsActive == null || u.IsActive == true)).ToList();

                Console.WriteLine($"Retrieved {students.Count} students from {allUsers.Count} total users");
                return students;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving students: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SupabaseUserNew>();
            }
        }

        /// <summary>
        /// Retrieves all instructors/teachers (roleId = 2).
        /// </summary>
        public async Task<List<SupabaseUserNew>> GetInstructorsAsync()
        {
            try
            {
                var allUsers = await GetAllUsersAsync();
                var instructors = allUsers.Where(u => u.UserTypeId == "2" && (u.IsActive == null || u.IsActive == true)).ToList();

                Console.WriteLine($"Retrieved {instructors.Count} instructors from {allUsers.Count} total users");
                return instructors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving instructors: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SupabaseUserNew>();
            }
        }

        /// <summary>
        /// Searches for users by name or email, optionally filtered by roleId.
        /// </summary>
        public async Task<List<SupabaseUserNew>> SearchUsersAsync(string searchTerm, string roleId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return roleId == null ? await GetAllUsersAsync() : 
                           roleId == "1" ? await GetStudentsAsync() : 
                           roleId == "2" ? await GetInstructorsAsync() : 
                           new List<SupabaseUserNew>();
                }

                var allUsers = await GetAllUsersAsync();
                var searchLower = searchTerm.ToLower();

                var filteredUsers = allUsers
                    .Where(u =>
                        ((u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                         (u.LastName != null && u.LastName.ToLower().Contains(searchLower)) ||
                         (u.Email != null && u.Email.ToLower().Contains(searchLower))) &&
                        (roleId == null || u.UserTypeId == roleId) &&
                        (u.IsActive == null || u.IsActive == true)
                    )
                    .ToList();

                Console.WriteLine($"Search for '{searchTerm}' (roleId: {roleId ?? "all"}) returned {filteredUsers.Count} users");
                return filteredUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching users: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SupabaseUserNew>();
            }
        }

        /// <summary>
        /// Retrieves all roles from the database.
        /// </summary>
        public async Task<List<RoleModel>> GetAllRolesAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var rolesQuery = await client
                    .From<RoleModel>()
                    .Get();

                var rolesList = rolesQuery?.Models ?? new List<RoleModel>();

                Console.WriteLine($"Retrieved {rolesList.Count} roles from database");
                return rolesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving roles: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<RoleModel>();
            }
        }

        /// <summary>
        /// Gets roles for a specific user by userId (bigint -> text).
        /// Performs join: users.id -> user_roles.userId -> roles.id
        /// </summary>
        public async Task<List<RoleModel>> GetUserRolesAsync(string userTypeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userTypeId))
                {
                    Console.WriteLine("Error: userTypeId is null or empty");
                    return new List<RoleModel>();
                }

                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Fetch user_roles records where userId matches (users.id as string)
                var userRolesQuery = await client
                    .From<UserRoleModel>()
                    .Where(ur => ur.UserId == userTypeId)
                    .Get();

                var userRoles = userRolesQuery?.Models ?? new List<UserRoleModel>();

                Console.WriteLine($"Query user_roles: userId={userTypeId}, found {userRoles.Count} role assignments");
                
                if (userRoles.Count == 0)
                {
                    Console.WriteLine($"No roles found for userId {userTypeId}");
                    return new List<RoleModel>();
                }

                // Get all roleIds for this user (converting from string to long)
                var roleIds = userRoles
                    .Select(ur => 
                    {
                        if (long.TryParse(ur.RoleId, out long roleId))
                            return roleId;
                        return 0;
                    })
                    .Where(id => id > 0)
                    .ToList();
                Console.WriteLine($"Found {roleIds.Count} role assignments for user {userTypeId}: {string.Join(", ", roleIds)}");

                // Fetch the role details for each roleId
                var allRoles = await GetAllRolesAsync();
                var userRoleDetails = allRoles
                    .Where(r => roleIds.Contains(r.Id))
                    .ToList();

                Console.WriteLine($"Retrieved {userRoleDetails.Count} role details for user {userTypeId}");
                return userRoleDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user roles: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<RoleModel>();
            }
        }

        /// <summary>
        /// Creates a new student account in Supabase Auth and adds student record to database.
        /// </summary>
     public async Task<bool> CreateStudentAsync(StudentCreateDto model)
    {
    try
{
       Console.WriteLine($"=== CreateStudentAsync (UserService) ===");
     Console.WriteLine($"Creating student: {model.FirstName} {model.LastName} ({model.Email})");
      Console.WriteLine($"  Program ID: {model.ProgramId}, Department ID: {model.DepartmentId}");

    // ✅ FIX: Pass Program and Department as ID strings (they'll be parsed in StudentService)
      // Convert DTO to StudentViewModel
var studentViewModel = new StudentViewModel
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
         YearLevel = (int)model.YearLevel,  // ✅ Cast decimal to int
    Program = model.ProgramId,  // ✅ Pass ID as string (will be parsed)
          Department = model.DepartmentId,  // ✅ Pass ID as string (will be parsed)
       EmergencyFirstName = model.EmergencyContactFirstName,
           EmergencyMiddleName = model.EmergencyContactMiddleName,
              EmergencyLastName = model.EmergencyContactLastName,
            EmergencySuffix = model.EmergencyContactSuffix,
            EmergencyContactNumber = model.EmergencyContactNumber,
Relationship = model.EmergencyContactRelationship
      };

              // Call StudentService to handle the creation
         var result = await _studentService.CreateStudentAsync(studentViewModel);

         Console.WriteLine($"Student creation result: {result}");
       Console.WriteLine($"=== End CreateStudentAsync ===\n");

         return result;
            }
       catch (Exception ex)
      {
   Console.WriteLine($"Error creating student: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        return false;
}
        }

        /// <summary>
        /// Creates a new teacher account in Supabase Auth and adds teacher record to database.
        /// </summary>
        public async Task<bool> CreateTeacherAsync(TeacherCreateDto model)
        {
            try
 {
   Console.WriteLine($"=== CreateTeacherAsync (UserService) ===");
   Console.WriteLine($"Creating teacher: {model.FirstName} {model.LastName} ({model.Email})");
      Console.WriteLine($"  Department ID: {model.DepartmentId}");

    // ✅ FIX: Map DTO to ViewModel and call TeacherService
      var teacherViewModel = new TeacherViewModel
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
          Department = model.DepartmentId  // Pass department ID
   };

   // Call TeacherService to handle the actual creation
        var result = await _teacherService.CreateTeacherAsync(teacherViewModel);

    Console.WriteLine($"Teacher creation result: {result}");
  Console.WriteLine($"=== End CreateTeacherAsync ===\n");

    return result;
   }
   catch (Exception ex)
    {
         Console.WriteLine($"✗ Error creating teacher: {ex.Message}");
     Console.WriteLine($"Stack Trace: {ex.StackTrace}");
      return false;
   }
  }
    }
}
