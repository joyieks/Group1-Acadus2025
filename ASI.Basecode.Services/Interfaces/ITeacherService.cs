using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Service interface for teacher-related operations.
    /// Provides methods to retrieve dashboard statistics and teacher-specific data.
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>
        /// Gets the total number of courses taught by a specific teacher.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>The count of courses where the teacher is the instructor.</returns>
        int GetTotalCoursesForTeacher(int teacherId);

        /// <summary>
        /// Gets the total number of activities across all courses taught by a teacher.
        /// Includes only activities from active (non-archived) courses.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>The count of all activities in the teacher's courses.</returns>
        int GetTotalActivitiesForTeacher(int teacherId);

        /// <summary>
        /// Gets the count of graded activities for a specific teacher.
        /// A graded activity is one where a submission has been scored and marked as "Graded".
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>The count of activity submissions that have been graded (score != null and status = "Graded").</returns>
        int GetGradedActivitiesCountForTeacher(int teacherId);

        /// <summary>
        /// Gets the total number of activities for a specific teacher within a date range (e.g., this week).
        /// Includes only activities with due dates within the specified range.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <param name="startDate">The start date of the range (typically Monday of the week).</param>
        /// <param name="endDate">The end date of the range (typically Sunday of the week).</param>
        /// <returns>The count of activities due within the date range.</returns>
        int GetTotalActivitiesForTeacherByWeek(int teacherId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the count of graded activities for a specific teacher within a date range (e.g., this week).
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <param name="startDate">The start date of the range (typically Monday of the week).</param>
        /// <param name="endDate">The end date of the range (typically Sunday of the week).</param>
        /// <returns>The count of graded submissions within the date range.</returns>
        int GetGradedActivitiesCountForTeacherByWeek(int teacherId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets all courses taught by a specific teacher.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>A collection of courses where the teacher is the instructor.</returns>
        IEnumerable<ASI.Basecode.Data.Models.Course> GetCoursesByTeacher(int teacherId);

        /// <summary>
        /// Gets all activities for a specific teacher across all their courses.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>A collection of activities from all courses taught by this teacher.</returns>
        IEnumerable<ASI.Basecode.Data.Models.Activity> GetActivitiesByTeacher(int teacherId);
    }
}

