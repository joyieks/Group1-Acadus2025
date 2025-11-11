using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for Course data access operations with Supabase.
    /// Handles all CRUD and query operations related to courses.
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Gets all courses taught by a specific teacher.
        /// Supabase Query: SELECT * FROM courses WHERE instructor = teacherId
        /// </summary>
        /// <param name="teacherId">The instructor's user ID (Course.instructor FK)</param>
        /// <returns>List of courses taught by the teacher</returns>
        Task<List<Course>> GetCoursesByTeacherAsync(int teacherId);

        /// <summary>
        /// Gets a specific course by its ID.
        /// Supabase Query: SELECT * FROM courses WHERE id = courseId LIMIT 1
        /// </summary>
        /// <param name="courseId">The course ID</param>
        /// <returns>Course object or null if not found</returns>
        Task<Course> GetCourseByIdAsync(int courseId);

        /// <summary>
        /// Gets all courses (use with caution - can be expensive).
        /// Supabase Query: SELECT * FROM courses
        /// </summary>
        /// <returns>List of all courses</returns>
        Task<List<Course>> GetAllCoursesAsync();

        /// <summary>
        /// Gets all active courses.
        /// Supabase Query: SELECT * FROM courses WHERE status = 'Active'
        /// </summary>
        /// <returns>List of active courses</returns>
        Task<List<Course>> GetActiveCoursesAsync();

        /// <summary>
        /// Gets all courses for a specific semester.
        /// Supabase Query: SELECT * FROM courses WHERE semesterId = semesterId
        /// </summary>
        /// <param name="semesterId">The semester ID</param>
        /// <returns>List of courses in the semester</returns>
        Task<List<Course>> GetCoursesBySemesterAsync(int semesterId);

        /// <summary>
        /// Creates a new course.
        /// Supabase: INSERT INTO courses (...)
        /// </summary>
        /// <param name="course">The course object to create</param>
        /// <returns>The created course with ID populated</returns>
        Task<Course> CreateCourseAsync(Course course);

        /// <summary>
        /// Updates an existing course.
        /// Supabase: UPDATE courses SET ... WHERE id = courseId
        /// </summary>
        /// <param name="course">The course object with updated values</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateCourseAsync(Course course);

        /// <summary>
        /// Deletes a course by ID.
        /// Supabase: DELETE FROM courses WHERE id = courseId
        /// </summary>
        /// <param name="courseId">The course ID to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteCourseAsync(int courseId);
    }
}
