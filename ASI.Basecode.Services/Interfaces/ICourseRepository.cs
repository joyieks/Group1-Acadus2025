using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for Course entity operations.
    /// Handles course management in the Acadus system.
    /// Courses belong to semesters and are taught by instructors (teachers).
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Gets all active courses.
        /// </summary>
        /// <returns>List of all courses</returns>
        Task<List<Course>> GetAllCoursesAsync();

        /// <summary>
        /// Gets a specific course by its ID.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>Course record or null if not found</returns>
        Task<Course> GetCourseByIdAsync(int courseId);

        /// <summary>
        /// Gets all courses for a specific semester.
        /// </summary>
        /// <param name="semesterId">The ID of the semester</param>
        /// <returns>List of courses offered in that semester</returns>
        Task<List<Course>> GetCoursesBySemesterAsync(int semesterId);

        /// <summary>
        /// Gets all courses taught by a specific instructor.
        /// Joins courses where instructor (FK to User.id) matches the teacherId.
        /// </summary>
        /// <param name="teacherId">The ID of the instructor (User.id)</param>
        /// <returns>List of courses where instructor = teacherId</returns>
        Task<List<Course>> GetCoursesByInstructorAsync(int teacherId);

        /// <summary>
        /// Gets all courses a student is enrolled in.
        /// Requires joining with course_enrollment table to find matching courses.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>List of courses where student is enrolled</returns>
        Task<List<Course>> GetCoursesByStudentAsync(int studentId);

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="course">The Course object to insert</param>
        /// <returns>The created Course with ID populated</returns>
        Task<Course> CreateCourseAsync(Course course);

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="course">The Course object with updated values</param>
        /// <returns>The updated Course</returns>
        Task<Course> UpdateCourseAsync(Course course);

        /// <summary>
        /// Gets the number of enrolled students in a course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>Count of enrolled students</returns>
        Task<int> GetEnrolledStudentCountAsync(int courseId);

        /// <summary>
        /// Checks if a course is at capacity.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>True if enrolled students >= capacity, false otherwise</returns>
        Task<bool> IsCourseAtCapacityAsync(int courseId);

        /// <summary>
        /// Gets course by course code.
        /// </summary>
        /// <param name="courseCode">The course code (e.g., CS101, 91299)</param>
        /// <returns>Course record or null if not found</returns>
        Task<Course> GetCourseByCourseCodeAsync(string courseCode);
    }
}
