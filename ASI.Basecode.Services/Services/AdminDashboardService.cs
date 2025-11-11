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
    /// Service implementation for admin dashboard operations.
    /// Aggregates system-wide statistics and management data.
    /// </summary>
    public class AdminDashboardService : ServiceBase, IAdminDashboardService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly ITeacherProfileRepository _teacherProfileRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ICourseEnrollmentRepository _enrollmentRepository;

        public AdminDashboardService(
            ILoggerFactory loggerFactory,
            ICourseRepository courseRepository,
            IActivityRepository activityRepository,
            IStudentProfileRepository studentProfileRepository,
            ITeacherProfileRepository teacherProfileRepository,
            IUserRoleRepository userRoleRepository,
            ICourseEnrollmentRepository enrollmentRepository)
            : base(loggerFactory)
        {
            _courseRepository = courseRepository;
            _activityRepository = activityRepository;
            _studentProfileRepository = studentProfileRepository;
            _teacherProfileRepository = teacherProfileRepository;
            _userRoleRepository = userRoleRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<int> GetTotalUsersAsync()
        {
            try
            {
                var allRoles = await _userRoleRepository.GetAllUserRolesAsync();
                // Count unique users across all roles
                return allRoles.Select(ur => ur.userId).Distinct().Count();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total users: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalCoursesAsync()
        {
            try
            {
                var courses = await _courseRepository.GetAllCoursesAsync();
                return courses?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total courses: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalStudentsAsync()
        {
            try
            {
                var profiles = await _studentProfileRepository.GetAllStudentProfilesAsync();
                return profiles?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total students: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalTeachersAsync()
        {
            try
            {
                var profiles = await _teacherProfileRepository.GetAllTeacherProfilesAsync();
                return profiles?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total teachers: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalActivitiesAsync()
        {
            try
            {
                var activities = await _activityRepository.GetAllActivitiesAsync();
                return activities?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total activities: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<User>> GetUsersByRoleAsync(int roleId)
        {
            try
            {
                var userRoles = await _userRoleRepository.GetUserRolesByRoleIdAsync(roleId);
                return userRoles?.Select(ur => new User { id = ur.userId }).ToList() ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting users by role {roleId}: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<int> GetEnrollmentCountByCoursesAsync(int courseId)
        {
            try
            {
                return await _enrollmentRepository.GetEnrollmentCountByCourseAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting enrollment count for course {courseId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Course>> GetAllCoursesWithEnrollmentAsync()
        {
            try
            {
                return await _courseRepository.GetAllCoursesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all courses: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<double> GetAverageEnrollmentAsync()
        {
            try
            {
                var courses = await _courseRepository.GetAllCoursesAsync();
                if (courses == null || courses.Count == 0)
                    return 0;

                double totalEnrollment = 0;
                foreach (var course in courses)
                {
                    var count = await _enrollmentRepository.GetEnrollmentCountByCourseAsync(course.id);
                    totalEnrollment += count;
                }

                return totalEnrollment / courses.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating average enrollment: {ex.Message}");
                return 0;
            }
        }
    }
}
