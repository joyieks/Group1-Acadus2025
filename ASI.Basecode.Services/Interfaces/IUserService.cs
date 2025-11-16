using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, ref User user);
        void AddUser(UserViewModel model);

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        Task<List<SupabaseUserNew>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves all students (roleId = 1).
        /// </summary>
        Task<List<SupabaseUserNew>> GetStudentsAsync();

        /// <summary>
        /// Retrieves all instructors/teachers (roleId = 2).
        /// </summary>
        Task<List<SupabaseUserNew>> GetInstructorsAsync();

        /// <summary>
        /// Searches for users by name or email, optionally filtered by roleId.
        /// </summary>
        Task<List<SupabaseUserNew>> SearchUsersAsync(string searchTerm, string roleId = null);

        /// <summary>
        /// Retrieves all roles from the database.
        /// </summary>
        Task<List<RoleModel>> GetAllRolesAsync();

        /// <summary>
        /// Gets roles for a specific user by userTypeId.
        /// Uses join query: users -> user_role -> roles
        /// </summary>
        Task<List<RoleModel>> GetUserRolesAsync(string userTypeId);

        /// <summary>
        /// Creates a new student account in Supabase Auth and adds student record to database.
        /// </summary>
        Task<bool> CreateStudentAsync(StudentCreateDto model);

        /// <summary>
        /// Creates a new teacher account in Supabase Auth and adds teacher record to database.
        /// </summary>
        Task<bool> CreateTeacherAsync(TeacherCreateDto model);
    }
}

