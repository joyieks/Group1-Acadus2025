using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services
{
    /// <summary>
    /// Service implementation for teacher dashboard operations.
    /// Aggregates data and statistics for teachers using repository interfaces.
    /// Follows the pattern: Service → Repository → Supabase
    /// </summary>
    public class TeacherDashboardService : ServiceBase, ITeacherDashboardService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IActivitySubmissionRepository _submissionRepository;

        public TeacherDashboardService(
            ILoggerFactory loggerFactory,
            ICourseRepository courseRepository,
            IActivityRepository activityRepository,
            IActivitySubmissionRepository submissionRepository)
            : base(loggerFactory)
        {
            _courseRepository = courseRepository;
            _activityRepository = activityRepository;
            _submissionRepository = submissionRepository;
        }

        /// <summary>
        /// Gets complete dashboard statistics for a teacher.
        /// Aggregates: total courses, total activities, graded activities.
        /// </summary>
        public async Task<DashboardStatistics> GetDashboardStatisticsAsync(int teacherId)
        {
            try
            {
                var totalCourses = await GetTotalCoursesAsync(teacherId);
                var totalActivities = await GetTotalActivitiesAsync(teacherId);
                var gradedActivities = await GetGradedActivitiesAsync(teacherId);

                return new DashboardStatistics
                {
                    TotalCoursesHandled = totalCourses,
                    TotalActivities = totalActivities,
                    GradedActivities = gradedActivities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting dashboard statistics for teacher {teacherId}: {ex.Message}");
                return new DashboardStatistics { TotalCoursesHandled = 0, TotalActivities = 0, GradedActivities = 0 };
            }
        }

        /// <summary>
        /// Gets the total number of active courses taught by this instructor.
        /// </summary>
        public async Task<int> GetTotalCoursesAsync(int teacherId)
        {
            try
            {
                var courses = await _courseRepository.GetCoursesByInstructorAsync(teacherId);
                return courses?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total courses for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the total number of non-archived activities across all teacher's courses.
        /// </summary>
        public async Task<int> GetTotalActivitiesAsync(int teacherId)
        {
            try
            {
                var activities = await _activityRepository.GetActivitiesByInstructorAsync(teacherId);
                return activities?.Count(a => !a.isArchived) ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total activities for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the count of activities that have at least one graded submission.
        /// An activity is considered "graded" if it has submissions with score assigned.
        /// </summary>
        public async Task<int> GetGradedActivitiesAsync(int teacherId)
        {
            try
            {
                var activities = await _activityRepository.GetActivitiesByInstructorAsync(teacherId);
                if (activities == null || activities.Count == 0)
                    return 0;

                int gradedCount = 0;
                foreach (var activity in activities)
                {
                    var gradedSubmissions = await _submissionRepository.GetGradedSubmissionCountByActivityAsync(activity.id);
                    if (gradedSubmissions > 0)
                        gradedCount++;
                }

                return gradedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting graded activities for teacher {teacherId}: {ex.Message}");
                return 0;
            }
        }
    }
}
