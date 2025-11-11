using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for TeacherProfile entity operations.
    /// Handles teacher-specific profile data in the Acadus system.
    /// Each user with teacher role has at most one TeacherProfile (UNIQUE constraint on userId).
    /// </summary>
    public interface ITeacherProfileRepository
    {
        /// <summary>
        /// Gets all teacher profiles.
        /// </summary>
        /// <returns>List of all teacher profiles</returns>
        Task<List<TeacherProfile>> GetAllTeacherProfilesAsync();

        /// <summary>
        /// Gets a specific teacher profile by its ID.
        /// </summary>
        /// <param name="profileId">The ID of the teacher profile</param>
        /// <returns>TeacherProfile record or null if not found</returns>
        Task<TeacherProfile> GetTeacherProfileByIdAsync(int profileId);

        /// <summary>
        /// Gets the teacher profile for a specific user.
        /// Since userId has UNIQUE constraint, each user has at most one profile.
        /// </summary>
        /// <param name="userId">The ID of the user (User.id)</param>
        /// <returns>TeacherProfile or null if user has no teacher profile</returns>
        Task<TeacherProfile> GetTeacherProfileByUserIdAsync(int userId);

        /// <summary>
        /// Gets all teacher profiles in a specific department.
        /// </summary>
        /// <param name="departmentId">The ID of the department</param>
        /// <returns>List of teacher profiles in the department</returns>
        Task<List<TeacherProfile>> GetTeacherProfilesByDepartmentAsync(int departmentId);

        /// <summary>
        /// Gets the count of teachers in a specific department.
        /// </summary>
        /// <param name="departmentId">The ID of the department</param>
        /// <returns>Count of teachers in department</returns>
        Task<int> GetTeacherCountByDepartmentAsync(int departmentId);

        /// <summary>
        /// Creates a new teacher profile.
        /// </summary>
        /// <param name="teacherProfile">The TeacherProfile object to insert</param>
        /// <returns>The created TeacherProfile with ID populated</returns>
        Task<TeacherProfile> CreateTeacherProfileAsync(TeacherProfile teacherProfile);

        /// <summary>
        /// Updates an existing teacher profile.
        /// </summary>
        /// <param name="teacherProfile">The TeacherProfile object with updated values</param>
        /// <returns>The updated TeacherProfile</returns>
        Task<TeacherProfile> UpdateTeacherProfileAsync(TeacherProfile teacherProfile);

        /// <summary>
        /// Checks if a user has a teacher profile.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>True if user has a teacher profile, false otherwise</returns>
        Task<bool> UserHasTeacherProfileAsync(int userId);
    }
}
