using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.Services
{
    /// <summary>
    /// Repository implementation for CourseEnrollment entity operations.
    /// Handles student enrollment in courses using Supabase queries.
    /// </summary>
    public class CourseEnrollmentRepository : ICourseEnrollmentRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public CourseEnrollmentRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<CourseEnrollment>> GetAllEnrollmentsAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>().Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all enrollments: {ex.Message}");
                return new List<CourseEnrollment>();
            }
        }

        public async Task<CourseEnrollment> GetEnrollmentByIdAsync(int enrollmentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Where(x => x.id == enrollmentId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching enrollment {enrollmentId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CourseEnrollment>> GetEnrollmentsByCourseAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Where(x => x.courseId == courseId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching enrollments for course {courseId}: {ex.Message}");
                return new List<CourseEnrollment>();
            }
        }

        public async Task<List<CourseEnrollment>> GetEnrollmentsByStudentAsync(int studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Where(x => x.userId == studentId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching enrollments for student {studentId}: {ex.Message}");
                return new List<CourseEnrollment>();
            }
        }

        public async Task<CourseEnrollment> GetEnrollmentByStudentAndCourseAsync(int courseId, int studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Where(x => x.courseId == courseId && x.userId == studentId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching enrollment for student {studentId} in course {courseId}: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetEnrollmentCountByCourseAsync(int courseId)
        {
            try
            {
                var enrollments = await GetEnrollmentsByCourseAsync(courseId);
                return enrollments.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting enrollments for course {courseId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetEnrollmentCountByStudentAsync(int studentId)
        {
            try
            {
                var enrollments = await GetEnrollmentsByStudentAsync(studentId);
                return enrollments.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting enrollments for student {studentId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<CourseEnrollment> CreateEnrollmentAsync(CourseEnrollment enrollment)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Insert(new List<CourseEnrollment> { enrollment });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating enrollment: {ex.Message}");
                return null;
            }
        }

        public async Task<CourseEnrollment> UpdateEnrollmentAsync(CourseEnrollment enrollment)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Update(enrollment);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating enrollment: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteEnrollmentAsync(int enrollmentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                await client.From<CourseEnrollment>()
                    .Where(x => x.id == enrollmentId)
                    .Delete();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting enrollment {enrollmentId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CourseEnrollment>> GetActiveEnrollmentsByCourseAsync(int courseId)
        {
            try
            {
                var enrollments = await GetEnrollmentsByCourseAsync(courseId);
                return enrollments.Where(e => e.enrollmentStatus == "Active" || e.enrollmentStatus == "Enrolled").ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching active enrollments for course {courseId}: {ex.Message}");
                return new List<CourseEnrollment>();
            }
        }

        public async Task<List<CourseEnrollment>> GetEnrollmentsByStatusAsync(string enrollmentStatus)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Where(x => x.enrollmentStatus == enrollmentStatus)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching enrollments with status {enrollmentStatus}: {ex.Message}");
                return new List<CourseEnrollment>();
            }
        }

        public async Task<bool> IsStudentEnrolledAsync(int courseId, int studentId)
        {
            try
            {
                var enrollment = await GetEnrollmentByStudentAndCourseAsync(courseId, studentId);
                return enrollment != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if student {studentId} is enrolled in course {courseId}: {ex.Message}");
                return false;
            }
        }
    }
}
