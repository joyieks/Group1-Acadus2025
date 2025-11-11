using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    /// <summary>
    /// Repository interface for Course entity operations.
    /// Provides methods to query and manage courses in the database.
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Gets all courses from the database.
        /// </summary>
        /// <returns>IQueryable collection of all courses.</returns>
        IQueryable<Course> GetCourses();

        /// <summary>
        /// Gets a specific course by its ID.
        /// </summary>
        /// <param name="courseId">The course ID to retrieve.</param>
        /// <returns>The course if found, null otherwise.</returns>
        Course GetCourseById(int courseId);

        /// <summary>
        /// Gets all courses taught by a specific instructor.
        /// </summary>
        /// <param name="instructorId">The User ID of the instructor.</param>
        /// <returns>IQueryable collection of courses taught by the instructor.</returns>
        IQueryable<Course> GetCoursesByInstructor(int instructorId);

        /// <summary>
        /// Checks if a course exists by ID.
        /// </summary>
        /// <param name="courseId">The course ID to check.</param>
        /// <returns>True if the course exists, false otherwise.</returns>
        bool CourseExists(int courseId);

        /// <summary>
        /// Adds a new course to the database.
        /// </summary>
        /// <param name="course">The course entity to add.</param>
        void AddCourse(Course course);

        /// <summary>
        /// Updates an existing course in the database.
        /// </summary>
        /// <param name="course">The course entity with updated values.</param>
        void UpdateCourse(Course course);

        /// <summary>
        /// Deletes a course from the database.
        /// </summary>
        /// <param name="courseId">The ID of the course to delete.</param>
        void DeleteCourse(int courseId);
    }
}
