using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service implementation for teacher-related operations.
    /// Provides methods to retrieve dashboard statistics and teacher-specific data.
    /// Uses repositories to fetch data from the database.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IActivitySubmissionRepository _submissionRepository;

        /// <summary>
        /// Initializes a new instance of the TeacherService class.
        /// </summary>
        /// <param name="courseRepository">The course repository for database access.</param>
        /// <param name="activityRepository">The activity repository for database access.</param>
        /// <param name="submissionRepository">The submission repository for database access.</param>
        public TeacherService(
            ICourseRepository courseRepository,
            IActivityRepository activityRepository,
            IActivitySubmissionRepository submissionRepository)
        {
            _courseRepository = courseRepository;
            _activityRepository = activityRepository;
            _submissionRepository = submissionRepository;
        }

        /// <summary>
        /// Gets the total number of courses taught by a specific teacher.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>The count of courses where the teacher is the instructor.</returns>
        public int GetTotalCoursesForTeacher(int teacherId)
        {
            try
            {
                return _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .Count();
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting total courses for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the total number of activities across all courses taught by a teacher.
        /// Includes only activities from active (non-archived) courses.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>The count of all activities in the teacher's courses.</returns>
        public int GetTotalActivitiesForTeacher(int teacherId)
        {
            try
            {
                // Get all courses for this teacher
                var courseIds = _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .Select(c => c.id)
                    .ToList();

                if (courseIds.Count == 0)
                {
                    return 0;
                }

                // Count all activities in those courses
                return _activityRepository
                    .GetActivities()
                    .Where(a => courseIds.Contains(a.courseId) && !a.isArchived)
                    .Count();
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting total activities for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the count of graded activities for a specific teacher.
        /// A graded activity is one where a submission has been scored and marked as "Graded".
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>The count of activity submissions that have been graded.</returns>
        public int GetGradedActivitiesCountForTeacher(int teacherId)
        {
            try
            {
                // Get all courses for this teacher
                var courseIds = _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .Select(c => c.id)
                    .ToList();

                if (courseIds.Count == 0)
                {
                    return 0;
                }

                // Get all activities in those courses
                var activityIds = _activityRepository
                    .GetActivities()
                    .Where(a => courseIds.Contains(a.courseId))
                    .Select(a => a.id)
                    .ToList();

                if (activityIds.Count == 0)
                {
                    return 0;
                }

                // Count submissions that are graded
                return _submissionRepository
                    .GetSubmissions()
                    .Where(s => activityIds.Contains(s.activityId) &&
                                s.submissionStatus == "Graded" &&
                                s.score.HasValue)
                    .Count();
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting graded activities count for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets all courses taught by a specific teacher.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>A collection of courses where the teacher is the instructor.</returns>
        public IEnumerable<Course> GetCoursesByTeacher(int teacherId)
        {
            try
            {
                return _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting courses for teacher {teacherId}: {ex.Message}");
                return new List<Course>();
            }
        }

        /// <summary>
        /// Gets all activities for a specific teacher across all their courses.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <returns>A collection of activities from all courses taught by this teacher.</returns>
        public IEnumerable<Activity> GetActivitiesByTeacher(int teacherId)
        {
            try
            {
                // Get all courses for this teacher
                var courseIds = _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .Select(c => c.id)
                    .ToList();

                if (courseIds.Count == 0)
                {
                    return new List<Activity>();
                }

                // Get all activities in those courses
                return _activityRepository
                    .GetActivities()
                    .Where(a => courseIds.Contains(a.courseId))
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting activities for teacher {teacherId}: {ex.Message}");
                return new List<Activity>();
            }
        }

        /// <summary>
        /// Gets the total number of activities for a teacher within a specific date range (typically a week).
        /// Only counts activities that are due within the specified date range and are not archived.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <param name="startDate">The start date of the range (typically Monday of the week).</param>
        /// <param name="endDate">The end date of the range (typically Sunday of the week).</param>
        /// <returns>The count of activities due within the date range.</returns>
        public int GetTotalActivitiesForTeacherByWeek(int teacherId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Get all courses for this teacher
                var courseIds = _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .Select(c => c.id)
                    .ToList();

                if (courseIds.Count == 0)
                {
                    return 0;
                }

                // Use the repository method that filters by date range
                return _activityRepository
                    .GetActivitiesByDueDateRange(courseIds, startDate, endDate)
                    .Count();
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting total activities by week for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the count of graded activities for a teacher within a specific date range (typically a week).
        /// Only counts submissions that have been graded and scored within the date range.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher (User.id).</param>
        /// <param name="startDate">The start date of the range (typically Monday of the week).</param>
        /// <param name="endDate">The end date of the range (typically Sunday of the week).</param>
        /// <returns>The count of graded submissions within the date range.</returns>
        public int GetGradedActivitiesCountForTeacherByWeek(int teacherId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Get all courses for this teacher
                var courseIds = _courseRepository
                    .GetCoursesByInstructor(teacherId)
                    .Select(c => c.id)
                    .ToList();

                if (courseIds.Count == 0)
                {
                    return 0;
                }

                // Get all activities in those courses that are due within the date range
                var activityIds = _activityRepository
                    .GetActivitiesByDueDateRange(courseIds, startDate, endDate)
                    .Select(a => a.id)
                    .ToList();

                if (activityIds.Count == 0)
                {
                    return 0;
                }

                // Use the repository method that counts graded submissions by date range
                return _submissionRepository
                    .GetGradedSubmissionCountByDateRange(activityIds, startDate, endDate);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error getting graded activities by week for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }
    }
}
