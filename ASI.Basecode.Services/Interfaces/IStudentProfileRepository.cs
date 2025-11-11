using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for StudentProfile entity operations.
    /// Handles student-specific profile data in the Acadus system.
    /// Each user with student role has at most one StudentProfile (UNIQUE constraint on userId).
    /// </summary>
    public interface IStudentProfileRepository
    {
        /// <summary>
        /// Gets all student profiles.
        /// </summary>
        /// <returns>List of all student profiles</returns>
        Task<List<StudentProfile>> GetAllStudentProfilesAsync();

        /// <summary>
        /// Gets a specific student profile by its ID.
        /// </summary>
        /// <param name="profileId">The ID of the student profile</param>
        /// <returns>StudentProfile record or null if not found</returns>
        Task<StudentProfile> GetStudentProfileByIdAsync(int profileId);

        /// <summary>
        /// Gets the student profile for a specific user.
        /// Since userId has UNIQUE constraint, each user has at most one profile.
        /// </summary>
        /// <param name="userId">The ID of the user (User.id)</param>
        /// <returns>StudentProfile or null if user has no student profile</returns>
        Task<StudentProfile> GetStudentProfileByUserIdAsync(int userId);

        /// <summary>
        /// Gets all student profiles in a specific program.
        /// </summary>
        /// <param name="programId">The ID of the program</param>
        /// <returns>List of student profiles enrolled in the program</returns>
        Task<List<StudentProfile>> GetStudentProfilesByProgramAsync(int programId);

        /// <summary>
        /// Gets all student profiles in a specific department.
        /// </summary>
        /// <param name="departmentId">The ID of the department</param>
        /// <returns>List of student profiles in the department</returns>
        Task<List<StudentProfile>> GetStudentProfilesByDepartmentAsync(int departmentId);

        /// <summary>
        /// Gets all student profiles by year level.
        /// </summary>
        /// <param name="yearLevel">The year level (1, 2, 3, or 4)</param>
        /// <returns>List of student profiles in the specified year</returns>
        Task<List<StudentProfile>> GetStudentProfilesByYearLevelAsync(int yearLevel);

        /// <summary>
        /// Gets all student profiles in a specific program AND year level.
        /// </summary>
        /// <param name="programId">The ID of the program</param>
        /// <param name="yearLevel">The year level (1, 2, 3, or 4)</param>
        /// <returns>List of student profiles matching both criteria</returns>
        Task<List<StudentProfile>> GetStudentProfilesByProgramAndYearAsync(int programId, int yearLevel);

        /// <summary>
        /// Creates a new student profile.
        /// </summary>
        /// <param name="studentProfile">The StudentProfile object to insert</param>
        /// <returns>The created StudentProfile with ID populated</returns>
        Task<StudentProfile> CreateStudentProfileAsync(StudentProfile studentProfile);

        /// <summary>
        /// Updates an existing student profile.
        /// </summary>
        /// <param name="studentProfile">The StudentProfile object with updated values</param>
        /// <returns>The updated StudentProfile</returns>
        Task<StudentProfile> UpdateStudentProfileAsync(StudentProfile studentProfile);

        /// <summary>
        /// Gets count of students in a specific program.
        /// </summary>
        /// <param name="programId">The ID of the program</param>
        /// <returns>Count of students in program</returns>
        Task<int> GetStudentCountByProgramAsync(int programId);

        /// <summary>
        /// Checks if a user has a student profile.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>True if user has a student profile, false otherwise</returns>
        Task<bool> UserHasStudentProfileAsync(int userId);
    }
}
