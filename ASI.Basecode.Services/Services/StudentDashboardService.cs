using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.Services
{
    /// <summary>
    /// Service implementation for student dashboard operations.
    /// Aggregates student-specific course and submission data.
    /// </summary>
    public class StudentDashboardService : ServiceBase, IStudentDashboardService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IActivitySubmissionRepository _submissionRepository;
        private readonly ICourseEnrollmentRepository _enrollmentRepository;

        public StudentDashboardService(
            ILoggerFactory loggerFactory,
            ICourseRepository courseRepository,
            IActivityRepository activityRepository,
            IActivitySubmissionRepository submissionRepository,
            ICourseEnrollmentRepository enrollmentRepository)
            : base(loggerFactory)
        {
            _courseRepository = courseRepository;
            _activityRepository = activityRepository;
            _submissionRepository = submissionRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<List<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            try
            {
                return await _courseRepository.GetCoursesByStudentAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting enrolled courses for student {studentId}: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<List<Activity>> GetPendingActivitiesAsync(int studentId)
        {
            try
            {
                var enrolledCourses = await GetEnrolledCoursesAsync(studentId);
                var pendingActivities = new List<Activity>();

                foreach (var course in enrolledCourses)
                {
                    var activities = await _activityRepository.GetUpcomingActivitiesByCourseAsync(course.id);
                    pendingActivities.AddRange(activities);
                }

                return pendingActivities.OrderBy(a => a.dueDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending activities for student {studentId}: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<List<ActivitySubmission>> GetStudentSubmissionsAsync(int studentId)
        {
            try
            {
                return await _submissionRepository.GetSubmissionsByStudentAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting submissions for student {studentId}: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }

        public async Task<double> GetStudentAverageScoreAsync(int studentId)
        {
            try
            {
                return await _submissionRepository.GetAverageScoreByStudentAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting average score for student {studentId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<ActivitySubmission>> GetCourseSubmissionsAsync(int studentId, int courseId)
        {
            try
            {
                return await _submissionRepository.GetSubmissionsByStudentAndCourseAsync(studentId, courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting submissions for student {studentId} in course {courseId}: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }
    }
}
