using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ICourseService
    {
        /// <summary>
        /// Retrieves all courses from the database.
        /// </summary>
        Task<List<CourseModel>> GetAllCoursesAsync();

        /// <summary>
        /// Retrieves a specific course by ID.
        /// </summary>
        Task<CourseModel> GetCourseByIdAsync(int courseId);

        /// <summary>
        /// Retrieves all active courses.
        /// </summary>
        Task<List<CourseModel>> GetActiveCoursesAsync();

        /// <summary>
        /// Retrieves courses for a specific student.
        /// </summary>
        Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId);

        /// <summary>
        /// Retrieves detailed information about a course for a student.
        /// </summary>
        Task<StudentCourseDetailsViewModel> GetCourseDetailsAsync(string studentId, string courseId);

        /// <summary>
        /// Retrieves courses taught by a specific instructor.
        /// </summary>
        Task<List<CourseModel>> GetCoursesByInstructorAsync(string instructorId);

        /// <summary>
        /// Retrieves all active instructors (users with roleId=2 and isActive=true).
        /// Returns tuples of (userTypeId, fullName).
        /// </summary>
        Task<List<(string UserTypeId, string FullName)>> GetActiveInstructorsAsync();

        /// <summary>
        /// Retrieves all semesters from the database.
        /// </summary>
        Task<List<SemesterModel>> GetAllSemestersAsync();

        /// <summary>
        /// Creates a new course with validation.
        /// </summary>
        Task<(bool Success, string Message, int? CourseId)> CreateCourseAsync(
            string code,
            string name, 
            string description,
            long credits,
            string level,
            long semesterId, 
            decimal maxCapacity, 
            string instructorId,
            string status = "Active");
    }
}
