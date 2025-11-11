using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for UserRole entity operations.
    /// Handles user-to-role assignments in the Acadus system.
    /// Supports many-to-many relationships via the user_roles junction table.
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Gets all roles assigned to a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user (User.id)</param>
        /// <returns>List of UserRole records where userId matches</returns>
        Task<List<UserRole>> GetUserRolesByUserIdAsync(int userId);

        /// <summary>
        /// Gets all users assigned to a specific role.
        /// </summary>
        /// <param name="roleId">The ID of the role (Role.id)</param>
        /// <returns>List of UserRole records where roleId matches</returns>
        Task<List<UserRole>> GetUserRolesByRoleIdAsync(int roleId);

        /// <summary>
        /// Gets a specific user-role assignment by ID.
        /// </summary>
        /// <param name="userRoleId">The ID of the user role record</param>
        /// <returns>UserRole record or null if not found</returns>
        Task<UserRole> GetUserRoleByIdAsync(int userRoleId);

        /// <summary>
        /// Creates a new user-role assignment.
        /// Supports users having multiple roles (e.g., User can be both Student and Admin).
        /// </summary>
        /// <param name="userRole">The UserRole object to insert</param>
        /// <returns>The created UserRole with ID populated</returns>
        Task<UserRole> CreateUserRoleAsync(UserRole userRole);

        /// <summary>
        /// Deletes a specific user-role assignment.
        /// </summary>
        /// <param name="userRoleId">The ID of the user role record to delete</param>
        /// <returns>True if successful, false if not found</returns>
        Task<bool> DeleteUserRoleAsync(int userRoleId);

        /// <summary>
        /// Checks if a user has a specific role.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="roleId">The ID of the role</param>
        /// <returns>True if user has the role, false otherwise</returns>
        Task<bool> UserHasRoleAsync(int userId, int roleId);

        /// <summary>
        /// Gets all user roles with complete user and role information.
        /// </summary>
        /// <returns>List of all UserRole assignments</returns>
        Task<List<UserRole>> GetAllUserRolesAsync();
    }
}
