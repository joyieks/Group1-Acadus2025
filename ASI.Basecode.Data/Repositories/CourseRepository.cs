using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Course entity operations.
    /// Inherits from BaseRepository to leverage common database access patterns.
    /// </summary>
    public class CourseRepository : BaseRepository, ICourseRepository
    {
        public CourseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Gets all courses from the database.
        /// </summary>
        /// <returns>IQueryable collection of all courses.</returns>
        public IQueryable<Course> GetCourses()
        {
            return this.GetDbSet<Course>();
        }

        /// <summary>
        /// Gets a specific course by its ID.
        /// </summary>
        /// <param name="courseId">The course ID to retrieve.</param>
        /// <returns>The course if found, null otherwise.</returns>
        public Course GetCourseById(int courseId)
        {
            return this.GetDbSet<Course>()
                .FirstOrDefault(c => c.id == courseId);
        }

        /// <summary>
        /// Gets all courses taught by a specific instructor.
        /// </summary>
        /// <param name="instructorId">The User ID of the instructor.</param>
        /// <returns>IQueryable collection of courses taught by the instructor.</returns>
        public IQueryable<Course> GetCoursesByInstructor(int instructorId)
        {
            return this.GetDbSet<Course>()
                .Where(c => c.instructor == instructorId);
        }

        /// <summary>
        /// Checks if a course exists by ID.
        /// </summary>
        /// <param name="courseId">The course ID to check.</param>
        /// <returns>True if the course exists, false otherwise.</returns>
        public bool CourseExists(int courseId)
        {
            return this.GetDbSet<Course>()
                .Any(c => c.id == courseId);
        }

        /// <summary>
        /// Adds a new course to the database.
        /// </summary>
        /// <param name="course">The course entity to add.</param>
        public void AddCourse(Course course)
        {
            this.GetDbSet<Course>().Add(course);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Updates an existing course in the database.
        /// </summary>
        /// <param name="course">The course entity with updated values.</param>
        public void UpdateCourse(Course course)
        {
            this.SetEntityState(course, Microsoft.EntityFrameworkCore.EntityState.Modified);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Deletes a course from the database.
        /// </summary>
        /// <param name="courseId">The ID of the course to delete.</param>
        public void DeleteCourse(int courseId)
        {
            var course = GetCourseById(courseId);
            if (course != null)
            {
                this.GetDbSet<Course>().Remove(course);
                UnitOfWork.SaveChanges();
            }
        }
    }
}
