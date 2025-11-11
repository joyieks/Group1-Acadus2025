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
    /// Repository implementation for Course entity operations.
    /// Handles course management using Supabase queries.
    /// </summary>
    public class CourseRepository : ICourseRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public CourseRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>().Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all courses: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<Course> GetCourseByIdAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>()
                    .Where(x => x.id == courseId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching course {courseId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Course>> GetCoursesBySemesterAsync(int semesterId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>()
                    .Where(x => x.semesterId == semesterId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching courses for semester {semesterId}: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<List<Course>> GetCoursesByInstructorAsync(int teacherId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>()
                    .Where(x => x.instructor == teacherId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching courses for instructor {teacherId}: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<List<Course>> GetCoursesByStudentAsync(int studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                
                // First get all enrollments for this student
                var enrollmentsResponse = await client.From<CourseEnrollment>()
                    .Where(x => x.userId == studentId)
                    .Get();
                
                if (!enrollmentsResponse.Models.Any())
                    return new List<Course>();
                
                // Get course IDs from enrollments
                var courseIds = enrollmentsResponse.Models.Select(e => e.courseId).ToList();
                
                // Fetch all courses with those IDs
                var courses = new List<Course>();
                foreach (var courseId in courseIds)
                {
                    var courseResponse = await client.From<Course>()
                        .Where(x => x.id == courseId)
                        .Get();
                    courses.AddRange(courseResponse.Models);
                }
                
                return courses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching courses for student {studentId}: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>()
                    .Insert(new List<Course> { course });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating course: {ex.Message}");
                return null;
            }
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>()
                    .Update(course);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating course: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetEnrolledStudentCountAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<CourseEnrollment>()
                    .Where(x => x.courseId == courseId)
                    .Get();
                return response.Models.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting enrolled students in course {courseId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> IsCourseAtCapacityAsync(int courseId)
        {
            try
            {
                var course = await GetCourseByIdAsync(courseId);
                if (course == null) return false;
                
                var enrolledCount = await GetEnrolledStudentCountAsync(courseId);
                return enrolledCount >= course.capacity;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking course capacity: {ex.Message}");
                return false;
            }
        }

        public async Task<Course> GetCourseByCourseCodeAsync(string courseCode)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Course>()
                    .Where(x => x.courseCode == courseCode)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching course by code {courseCode}: {ex.Message}");
                return null;
            }
        }
    }
}
