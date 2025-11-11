using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Repository interface for CourseEnrollment entity operations.
    /// Handles student enrollment in courses in the Acadus system.
    /// Tracks which students are enrolled in which courses and their enrollment status.
    /// </summary>
    public interface ICourseEnrollmentRepository
    {
        /// <summary>
        /// Gets all course enrollments.
        /// </summary>
        /// <returns>List of all enrollments</returns>
        Task<List<CourseEnrollment>> GetAllEnrollmentsAsync();

        /// <summary>
        /// Gets a specific enrollment by its ID.
        /// </summary>
        /// <param name="enrollmentId">The ID of the enrollment</param>
        /// <returns>CourseEnrollment record or null if not found</returns>
        Task<CourseEnrollment> GetEnrollmentByIdAsync(int enrollmentId);

        /// <summary>
        /// Gets all enrollments for a specific course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of enrollments in the course</returns>
        Task<List<CourseEnrollment>> GetEnrollmentsByCourseAsync(int courseId);

        /// <summary>
        /// Gets all enrollments for a specific student.
        /// </summary>
        /// <param name="studentId">The ID of the student (User.id)</param>
        /// <returns>List of courses student is enrolled in</returns>
        Task<List<CourseEnrollment>> GetEnrollmentsByStudentAsync(int studentId);

        /// <summary>
        /// Gets a specific student's enrollment in a specific course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <param name="studentId">The ID of the student</param>
        /// <returns>CourseEnrollment or null if not found (UNIQUE constraint ensures max 1)</returns>
        Task<CourseEnrollment> GetEnrollmentByStudentAndCourseAsync(int courseId, int studentId);

        /// <summary>
        /// Gets the count of enrolled students in a course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>Count of enrolled students</returns>
        Task<int> GetEnrollmentCountByCourseAsync(int courseId);

        /// <summary>
        /// Gets the count of courses a student is enrolled in.
        /// </summary>
        /// <param name="studentId">The ID of the student</param>
        /// <returns>Count of enrolled courses</returns>
        Task<int> GetEnrollmentCountByStudentAsync(int studentId);

        /// <summary>
        /// Creates a new enrollment.
        /// Automatically checks UNIQUE(courseId, studentId) constraint.
        /// </summary>
        /// <param name="enrollment">The CourseEnrollment object to insert</param>
        /// <returns>The created CourseEnrollment with ID populated</returns>
        Task<CourseEnrollment> CreateEnrollmentAsync(CourseEnrollment enrollment);

        /// <summary>
        /// Updates an existing enrollment (e.g., changing enrollment status).
        /// </summary>
        /// <param name="enrollment">The CourseEnrollment object with updated values</param>
        /// <returns>The updated CourseEnrollment</returns>
        Task<CourseEnrollment> UpdateEnrollmentAsync(CourseEnrollment enrollment);

        /// <summary>
        /// Removes a student from a course (deletes enrollment record).
        /// </summary>
        /// <param name="enrollmentId">The ID of the enrollment to delete</param>
        /// <returns>True if successful, false if not found</returns>
        Task<bool> DeleteEnrollmentAsync(int enrollmentId);

        /// <summary>
        /// Gets all active enrollments for a course.
        /// Where enrollmentStatus = "Active" or "Enrolled".
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <returns>List of active enrollments</returns>
        Task<List<CourseEnrollment>> GetActiveEnrollmentsByCourseAsync(int courseId);

        /// <summary>
        /// Gets all enrollments with a specific status.
        /// </summary>
        /// <param name="enrollmentStatus">The enrollment status (e.g., "Active", "Dropped")</param>
        /// <returns>List of enrollments with that status</returns>
        Task<List<CourseEnrollment>> GetEnrollmentsByStatusAsync(string enrollmentStatus);

        /// <summary>
        /// Checks if a student is enrolled in a course.
        /// </summary>
        /// <param name="courseId">The ID of the course</param>
        /// <param name="studentId">The ID of the student</param>
        /// <returns>True if student is enrolled, false otherwise</returns>
        Task<bool> IsStudentEnrolledAsync(int courseId, int studentId);
    }
}
