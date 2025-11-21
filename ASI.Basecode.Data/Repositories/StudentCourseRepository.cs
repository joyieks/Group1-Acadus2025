using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;




namespace ASI.Basecode.Data.Repositories
{
    public class StudentCourseRepository : IStudentCourseRepository
    {
        private readonly Client _supabaseClient;

        public StudentCourseRepository(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<CourseModel>> GetCoursesByStudentIdAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return new List<CourseModel>();

            // 1. Get active enrollments for this student
            var enrollmentsResponse = await _supabaseClient
                .From<EnrollmentModel>()
                .Where(e => e.StudentId == studentId && e.Status == "Active")
                .Get();

            var enrolledCourseIds = enrollmentsResponse.Models.Select(e => e.CourseId).ToList();

            if (!enrolledCourseIds.Any())
                return new List<CourseModel>();

            // 2. Fetch all courses and filter in memory
            var allCoursesResponse = await _supabaseClient
                .From<CourseModel>()
                .Filter("id", Operator.In, enrolledCourseIds.ToArray())
                .Get();


            var enrolledCourses = allCoursesResponse.Models
                .ToList();

            return enrolledCourses;
        }

        public async Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId)
        {
            // Get all activities for the course (filter IsVisible in memory)
           var res = await _supabaseClient
                .From<ActivityModel>()
                .Filter("courseId", Operator.Equals, courseId)
                .Filter("isVisible", Operator.Equals, "true")
                .Get();

            var activities = res.Models
                .OrderBy(a => a.DueDate)
                .ToList();

            
            return activities;
        }

        public async Task<SupabaseUserNew> GetUserByUserTypeIdAsync(string userTypeId)
        {
            var res = await _supabaseClient
                .From<SupabaseUserNew>()
                .Where(u => u.UserTypeId == userTypeId)
                .Get();

            return res.Models.FirstOrDefault();
        }



        public async Task<List<ActivitySubmissionModel>> GetSubmissionsByStudentAndCourseAsync(string studentId, long courseId)
        {
            var activities = await GetActivitiesByCourseIdAsync(courseId);
            var activityIds = activities.Select(a => a.Id).ToList();

            if (!activityIds.Any())
                return new List<ActivitySubmissionModel>();

            var res = await _supabaseClient
                .From<ActivitySubmissionModel>()
                .Where(s => s.StudentId == studentId)
                .Filter("activityId", Operator.In, activityIds.ToArray())
                .Get();

            return res.Models;
        }

        



    }
}
